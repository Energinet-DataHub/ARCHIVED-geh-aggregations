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
from geh_stream.aggregation_utils.aggregators import aggregate_per_ga, aggregate_per_ga_and_brp, aggregate_per_ga_and_es
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
import pytest
import pandas as pd

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)


@pytest.fixture(scope="module")
def settled_schema():
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType()) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType()) \
        .add("time_window",
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add("sum_quantity", DecimalType(20, 1))


@pytest.fixture(scope="module")
def agg_result_factory(spark, settled_schema):
    def factory():
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": ["1", "1", "1", "1", "1", "2"],
            "BalanceResponsibleParty_MarketParticipant_mRID": ["1", "2", "1", "2", "1", "1"],
            "EnergySupplier_MarketParticipant_mRID": ["1", "2", "3", "4", "5", "6"],
            "time_window": [
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 1, 0), "end": datetime(2020, 1, 1, 2, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
            ],
            "sum_quantity": [Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0)],
        })

        return spark.createDataFrame(pandas_df, schema=settled_schema)
    return factory


def test_hourly_settled_consumption_summarizes_correctly_on_grid_area_within_same_time_window(agg_result_factory):
    df = agg_result_factory()

    aggregated_df = aggregate_per_ga(df)

    assert aggregated_df.collect()[0]["sum_quantity"] == Decimal("4.0") and \
        aggregated_df.collect()[0]["MeteringGridArea_Domain_mRID"] == "1" and \
        aggregated_df.collect()[0]["time_window"]["start"] == datetime(2020, 1, 1, 0, 0) and \
        aggregated_df.collect()[0]["time_window"]["end"] == datetime(2020, 1, 1, 1, 0)


def test_hourly_settled_consumption_summarizes_correctly_on_grid_area_with_different_time_window(agg_result_factory):
    df = agg_result_factory()

    aggregated_df = aggregate_per_ga(df)

    assert aggregated_df.collect()[1]["sum_quantity"] == Decimal("1.0") and \
        aggregated_df.collect()[1]["MeteringGridArea_Domain_mRID"] == "1" and \
        aggregated_df.collect()[1]["time_window"]["start"] == datetime(2020, 1, 1, 1, 0) and \
        aggregated_df.collect()[1]["time_window"]["end"] == datetime(2020, 1, 1, 2, 0)


def test_hourly_settled_consumption_summarizes_correctly_on_grid_area_with_same_time_window_as_other_grid_area(agg_result_factory):
    df = agg_result_factory()

    aggregated_df = aggregate_per_ga(df)

    assert aggregated_df.collect()[2]["sum_quantity"] == Decimal("1.0") and \
        aggregated_df.collect()[2]["MeteringGridArea_Domain_mRID"] == "2" and \
        aggregated_df.collect()[2]["time_window"]["start"] == datetime(2020, 1, 1, 0, 0) and \
        aggregated_df.collect()[2]["time_window"]["end"] == datetime(2020, 1, 1, 1, 0)


def test_production_calculation_per_ga_and_es(agg_result_factory):
    df = agg_result_factory()
    aggregated_df = aggregate_per_ga_and_es(df)
    assert len(aggregated_df.columns) == 4
    assert aggregated_df.collect()[0]['MeteringGridArea_Domain_mRID'] == '1'
    assert aggregated_df.collect()[0]['EnergySupplier_MarketParticipant_mRID'] == '1'
    assert aggregated_df.collect()[0]['sum_quantity'] == Decimal(1)
    assert aggregated_df.collect()[1]['sum_quantity'] == Decimal(1)
    assert aggregated_df.collect()[2]['sum_quantity'] == Decimal(1)
    assert aggregated_df.collect()[3]['sum_quantity'] == Decimal(1)
    assert aggregated_df.collect()[4]['sum_quantity'] == Decimal(1)
    assert aggregated_df.collect()[5]['sum_quantity'] == Decimal(1)


def test_production_calculation_per_ga_and_brp(agg_result_factory):
    df = agg_result_factory()
    aggregated_df = aggregate_per_ga_and_brp(df)
    assert len(aggregated_df.columns) == 4
    assert aggregated_df.collect()[0]['MeteringGridArea_Domain_mRID'] == '1'
    assert aggregated_df.collect()[0]['sum_quantity'] == Decimal(2)
    assert aggregated_df.collect()[1]['sum_quantity'] == Decimal(1)
    assert aggregated_df.collect()[2]['sum_quantity'] == Decimal(2)
    assert aggregated_df.collect()[3]['sum_quantity'] == Decimal(1)


def test_production_calculation_per_ga(agg_result_factory):
    df = agg_result_factory()
    aggregated_df = aggregate_per_ga(df)
    assert len(aggregated_df.columns) == 3
    assert aggregated_df.collect()[0]['MeteringGridArea_Domain_mRID'] == '1'
    assert aggregated_df.collect()[0]['sum_quantity'] == Decimal(4)
    assert aggregated_df.collect()[1]['sum_quantity'] == Decimal(1)
    assert aggregated_df.collect()[2]['sum_quantity'] == Decimal(1)
