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

from numpy import append
from geh_stream.aggregation_utils.aggregators import calculate_total_consumption
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
import pytest
import pandas as pd
from geh_stream.codelists import Quality


@pytest.fixture(scope="module")
def net_exchange_schema():
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("time_window",
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add("in_sum", DecimalType(20, 1)) \
        .add("out_sum", DecimalType(20, 1)) \
        .add("sum_quantity", DecimalType(20, 1)) \
        .add("aggregated_quality", StringType())


@pytest.fixture(scope="module")
def agg_net_exchange_factory(spark, net_exchange_schema):
    def factory():
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": ["1", "1", "1", "1", "1", "2"],
            "time_window": [
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 1, 0), "end": datetime(2020, 1, 1, 2, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
            ],
            "in_sum": [Decimal(2.0), Decimal(2.0), Decimal(2.0), Decimal(2.0), Decimal(2.0), Decimal(2.0)],
            "out_sum": [Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0)],
            "sum_quantity": [Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0)],
            "aggregated_quality": ["56", "56", "56", "56", "QM", "56"]
        })

        return spark.createDataFrame(pandas_df, schema=net_exchange_schema)
    return factory


@pytest.fixture(scope="module")
def production_schema():
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("time_window",
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add("sum_quantity", DecimalType(20, 1)) \
        .add("aggregated_quality", StringType())


@pytest.fixture(scope="module")
def agg_production_factory(spark, production_schema):
    def factory():
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": ["1", "1", "1", "1", "1", "2"],
            "time_window": [
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 1, 0), "end": datetime(2020, 1, 1, 2, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
            ],
            "sum_quantity": [Decimal(1.0), Decimal(2.0), Decimal(3.0), Decimal(4.0), Decimal(5.0), Decimal(6.0)],
            "aggregated_quality": ["56", "56", "56", "56", "E01", "56"]
        })

        return spark.createDataFrame(pandas_df, schema=production_schema)
    return factory


@pytest.fixture(scope="module")
def agg_total_production_factory(spark, production_schema):
    def factory(quality):
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": [],
            "time_window": [],
            "sum_quantity": [],
            "aggregated_quality": []})

        pandas_df = pandas_df.append({
            "MeteringGridArea_Domain_mRID": "1",
            "time_window": {
                "start": datetime(2020, 1, 1, 0, 0),
                "end": datetime(2020, 1, 1, 1, 0)
                },
            "sum_quantity": Decimal(1.0),
            "aggregated_quality": quality
        }, ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=production_schema)
    return factory


@pytest.fixture(scope="module")
def agg_total_net_exchange_factory(spark, net_exchange_schema):
    def factory(quality):
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": [],
            "time_window": [],
            "in_sum": [],
            "out_sum": [],
            "sum_quantity": [],
            "aggregated_quality": []
        })

        pandas_df = pandas_df.append({
            "MeteringGridArea_Domain_mRID": "1",
            "time_window": {
                "start": datetime(2020, 1, 1, 0, 0),
                "end": datetime(2020, 1, 1, 1, 0)
                },
            "in_sum": Decimal(1.0),
            "out_sum": Decimal(1.0),
            "sum_quantity": Decimal(1.0),
            "aggregated_quality": quality
        }, ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=net_exchange_schema)
    return factory


def test_grid_area_total_consumption(agg_net_exchange_factory, agg_production_factory):
    net_exchange_df = agg_net_exchange_factory()
    production_df = agg_production_factory()
    aggregated_df = calculate_total_consumption(net_exchange_df, production_df)

    assert aggregated_df.collect()[0]["total_consumption"] == Decimal("14.0") and \
        aggregated_df.collect()[1]["total_consumption"] == Decimal("6.0") and \
        aggregated_df.collect()[2]["total_consumption"] == Decimal("7.0")


@pytest.mark.parametrize("prod_quality, ex_quality, expected_quality",[ \
    (Quality.estimated.value, Quality.estimated.value, Quality.estimated.value), \
    (Quality.estimated.value, Quality.quantity_missing.value, Quality.estimated.value), \
    (Quality.estimated.value, Quality.as_read.value, Quality.estimated.value), \
    (Quality.quantity_missing.value, Quality.quantity_missing.value, Quality.estimated.value), \
    (Quality.quantity_missing.value, Quality.as_read.value, Quality.estimated.value), \
    (Quality.as_read.value, Quality.as_read.value, Quality.as_read.value)
    ])
def test_aggregated_quality(
    agg_total_production_factory,
    agg_total_net_exchange_factory,
    prod_quality, ex_quality,
    expected_quality
                            ):

    prod_df = agg_total_production_factory(prod_quality)
    ex_df = agg_total_net_exchange_factory(ex_quality)

    result_df = calculate_total_consumption(ex_df, prod_df)

    assert result_df.collect()[0]["aggregated_quality"] == expected_quality
