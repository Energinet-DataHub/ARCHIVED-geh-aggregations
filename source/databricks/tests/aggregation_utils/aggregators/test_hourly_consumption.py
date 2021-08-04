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
from geh_stream.codelists import Colname
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
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.balance_responsible_id, StringType()) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.time_window,
             StructType()
             .add(Colname.time_window_start, TimestampType())
             .add(Colname.time_window_end, TimestampType()),
             False) \
        .add(Colname.sum_quantity, DecimalType(20, 1)) \
        .add(Colname.aggregated_quality, StringType())


@pytest.fixture(scope="module")
def agg_result_factory(spark, settled_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["1", "1", "1", "1", "1", "2"],
            Colname.balance_responsible_id: ["1", "2", "1", "2", "1", "1"],
            Colname.energy_supplier_id: ["1", "2", "3", "4", "5", "6"],
            Colname.time_window: [
                {Colname.time_window_start: datetime(2020, 1, 1, 0, 0), Colname.time_window_end: datetime(2020, 1, 1, 1, 0)},
                {Colname.time_window_start: datetime(2020, 1, 1, 0, 0), Colname.time_window_end: datetime(2020, 1, 1, 1, 0)},
                {Colname.time_window_start: datetime(2020, 1, 1, 0, 0), Colname.time_window_end: datetime(2020, 1, 1, 1, 0)},
                {Colname.time_window_start: datetime(2020, 1, 1, 0, 0), Colname.time_window_end: datetime(2020, 1, 1, 1, 0)},
                {Colname.time_window_start: datetime(2020, 1, 1, 1, 0), Colname.time_window_end: datetime(2020, 1, 1, 2, 0)},
                {Colname.time_window_start: datetime(2020, 1, 1, 0, 0), Colname.time_window_end: datetime(2020, 1, 1, 1, 0)}
            ],
            Colname.sum_quantity: [Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0)],
            Colname.aggregated_quality: [Quality.estimated.value, Quality.estimated.value, Quality.estimated.value, Quality.estimated.value, Quality.estimated.value, Quality.estimated.value]
        })

        return spark.createDataFrame(pandas_df, schema=settled_schema)
    return factory


def test_hourly_settled_consumption_summarizes_correctly_on_grid_area_within_same_time_window(agg_result_factory):
    df = agg_result_factory()

    aggregated_df = aggregate_per_ga(df).sort(Colname.grid_area, Colname.time_window)

    assert aggregated_df.collect()[0][Colname.sum_quantity] == Decimal("4.0") and \
        aggregated_df.collect()[0][Colname.grid_area] == "1" and \
        aggregated_df.collect()[0][Colname.time_window]["start"] == datetime(2020, 1, 1, 0, 0) and \
        aggregated_df.collect()[0][Colname.time_window]["end"] == datetime(2020, 1, 1, 1, 0)


def test_hourly_settled_consumption_summarizes_correctly_on_grid_area_with_different_time_window(agg_result_factory):
    df = agg_result_factory()

    aggregated_df = aggregate_per_ga(df).sort(Colname.grid_area, Colname.time_window)

    assert aggregated_df.collect()[1][Colname.sum_quantity] == Decimal("1.0") and \
        aggregated_df.collect()[1][Colname.grid_area] == "1" and \
        aggregated_df.collect()[1][Colname.time_window]["start"] == datetime(2020, 1, 1, 1, 0) and \
        aggregated_df.collect()[1][Colname.time_window]["end"] == datetime(2020, 1, 1, 2, 0)


def test_hourly_settled_consumption_summarizes_correctly_on_grid_area_with_same_time_window_as_other_grid_area(agg_result_factory):
    df = agg_result_factory()

    aggregated_df = aggregate_per_ga(df).sort(Colname.grid_area, Colname.time_window)

    assert aggregated_df.collect()[2][Colname.sum_quantity] == Decimal("1.0") and \
        aggregated_df.collect()[2][Colname.grid_area] == "2" and \
        aggregated_df.collect()[2][Colname.time_window]["start"] == datetime(2020, 1, 1, 0, 0) and \
        aggregated_df.collect()[2][Colname.time_window]["end"] == datetime(2020, 1, 1, 1, 0)


def test_production_calculation_per_ga_and_es(agg_result_factory):
    df = agg_result_factory()
    aggregated_df = aggregate_per_ga_and_es(df).sort(Colname.grid_area, Colname.energy_supplier_id, Colname.time_window)
    assert len(aggregated_df.columns) == 5
    assert aggregated_df.collect()[0][Colname.grid_area] == "1"
    assert aggregated_df.collect()[0][Colname.energy_supplier_id] == "1"
    assert aggregated_df.collect()[0][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df.collect()[1][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df.collect()[2][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df.collect()[3][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df.collect()[4][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df.collect()[5][Colname.sum_quantity] == Decimal(1)


def test_production_calculation_per_ga_and_brp(agg_result_factory):
    df = agg_result_factory()
    aggregated_df = aggregate_per_ga_and_brp(df).sort(Colname.grid_area, Colname.balance_responsible_id, Colname.time_window)
    assert len(aggregated_df.columns) == 5
    assert aggregated_df.collect()[0][Colname.grid_area] == "1"
    assert aggregated_df.collect()[0][Colname.balance_responsible_id] == "1"
    assert aggregated_df.collect()[0][Colname.sum_quantity] == Decimal(2)
    assert aggregated_df.collect()[1][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df.collect()[2][Colname.sum_quantity] == Decimal(2)
    assert aggregated_df.collect()[3][Colname.sum_quantity] == Decimal(1)


def test_production_calculation_per_ga(agg_result_factory):
    df = agg_result_factory()
    aggregated_df = aggregate_per_ga(df).sort(Colname.grid_area, Colname.time_window)
    assert len(aggregated_df.columns) == 4
    assert aggregated_df.collect()[0][Colname.grid_area] == "1"
    assert aggregated_df.collect()[0][Colname.sum_quantity] == Decimal(4)
    assert aggregated_df.collect()[1][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df.collect()[2][Colname.sum_quantity] == Decimal(1)
