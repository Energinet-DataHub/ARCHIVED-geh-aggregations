# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from decimal import Decimal
from datetime import datetime
from geh_stream.codelists import Names
from geh_stream.aggregation_utils.aggregators import aggregate_per_ga, aggregate_per_ga_and_brp, aggregate_per_ga_and_es
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
import pytest
import pandas as pd
from geh_stream.codelists import Quality

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)


@pytest.fixture(scope="module")
def settled_schema():
    return StructType() \
        .add(Names.grid_area.value, StringType(), False) \
        .add(Names.balance_responsible_id.value, StringType()) \
        .add(Names.energy_supplier_id.value, StringType()) \
        .add(Names.time_window.value,
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add(Names.sum_quantity.value, DecimalType(20, 1)) \
        .add(Names.aggregated_quality.value, StringType())


@pytest.fixture(scope="module")
def agg_result_factory(spark, settled_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Names.grid_area.value: ["1", "1", "1", "1", "1", "2"],
            Names.balance_responsible_id.value: ["1", "2", "1", "2", "1", "1"],
            Names.energy_supplier_id.value: ["1", "2", "3", "4", "5", "6"],
            Names.time_window.value: [
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 1, 0), "end": datetime(2020, 1, 1, 2, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
            ],
            Names.sum_quantity.value: [Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0)],
            Names.aggregated_quality.value: [Quality.estimated.value, Quality.estimated.value, Quality.estimated.value, Quality.estimated.value, Quality.estimated.value, Quality.estimated.value]
        })

        return spark.createDataFrame(pandas_df, schema=settled_schema)
    return factory


def test_hourly_settled_consumption_summarizes_correctly_on_grid_area_within_same_time_window(agg_result_factory):
    df = agg_result_factory()

    aggregated_df = aggregate_per_ga(df).sort(Names.grid_area.value, Names.time_window.value)

    assert aggregated_df.collect()[0][Names.sum_quantity.value] == Decimal("4.0") and \
        aggregated_df.collect()[0][Names.grid_area.value] == "1" and \
        aggregated_df.collect()[0][Names.time_window.value]["start"] == datetime(2020, 1, 1, 0, 0) and \
        aggregated_df.collect()[0][Names.time_window.value]["end"] == datetime(2020, 1, 1, 1, 0)


def test_hourly_settled_consumption_summarizes_correctly_on_grid_area_with_different_time_window(agg_result_factory):
    df = agg_result_factory()

    aggregated_df = aggregate_per_ga(df).sort(Names.grid_area.value, Names.time_window.value)

    assert aggregated_df.collect()[1][Names.sum_quantity.value] == Decimal("1.0") and \
        aggregated_df.collect()[1][Names.grid_area.value] == "1" and \
        aggregated_df.collect()[1][Names.time_window.value]["start"] == datetime(2020, 1, 1, 1, 0) and \
        aggregated_df.collect()[1][Names.time_window.value]["end"] == datetime(2020, 1, 1, 2, 0)


def test_hourly_settled_consumption_summarizes_correctly_on_grid_area_with_same_time_window_as_other_grid_area(agg_result_factory):
    df = agg_result_factory()

    aggregated_df = aggregate_per_ga(df).sort(Names.grid_area.value, Names.time_window.value)

    assert aggregated_df.collect()[2][Names.sum_quantity.value] == Decimal("1.0") and \
        aggregated_df.collect()[2][Names.grid_area.value] == "2" and \
        aggregated_df.collect()[2][Names.time_window.value]["start"] == datetime(2020, 1, 1, 0, 0) and \
        aggregated_df.collect()[2][Names.time_window.value]["end"] == datetime(2020, 1, 1, 1, 0)


def test_production_calculation_per_ga_and_es(agg_result_factory):
    df = agg_result_factory()
    aggregated_df = aggregate_per_ga_and_es(df).sort(Names.grid_area.value, Names.energy_supplier_id.value, Names.time_window.value)
    assert len(aggregated_df.columns) == 5
    assert aggregated_df.collect()[0][Names.grid_area.value] == "1"
    assert aggregated_df.collect()[0][Names.energy_supplier_id.value] == "1"
    assert aggregated_df.collect()[0][Names.sum_quantity.value] == Decimal(1)
    assert aggregated_df.collect()[1][Names.sum_quantity.value] == Decimal(1)
    assert aggregated_df.collect()[2][Names.sum_quantity.value] == Decimal(1)
    assert aggregated_df.collect()[3][Names.sum_quantity.value] == Decimal(1)
    assert aggregated_df.collect()[4][Names.sum_quantity.value] == Decimal(1)
    assert aggregated_df.collect()[5][Names.sum_quantity.value] == Decimal(1)


def test_production_calculation_per_ga_and_brp(agg_result_factory):
    df = agg_result_factory()
    aggregated_df = aggregate_per_ga_and_brp(df).sort(Names.grid_area.value, Names.balance_responsible_id.value, Names.time_window.value)
    assert len(aggregated_df.columns) == 5
    assert aggregated_df.collect()[0][Names.grid_area.value] == "1"
    assert aggregated_df.collect()[0][Names.balance_responsible_id.value] == "1"
    assert aggregated_df.collect()[0][Names.sum_quantity.value] == Decimal(2)
    assert aggregated_df.collect()[1][Names.sum_quantity.value] == Decimal(1)
    assert aggregated_df.collect()[2][Names.sum_quantity.value] == Decimal(2)
    assert aggregated_df.collect()[3][Names.sum_quantity.value] == Decimal(1)


def test_production_calculation_per_ga(agg_result_factory):
    df = agg_result_factory()
    aggregated_df = aggregate_per_ga(df).sort(Names.grid_area.value, Names.time_window.value)
    assert len(aggregated_df.columns) == 4
    assert aggregated_df.collect()[0][Names.grid_area.value] == "1"
    assert aggregated_df.collect()[0][Names.sum_quantity.value] == Decimal(4)
    assert aggregated_df.collect()[1][Names.sum_quantity.value] == Decimal(1)
    assert aggregated_df.collect()[2][Names.sum_quantity.value] == Decimal(1)
