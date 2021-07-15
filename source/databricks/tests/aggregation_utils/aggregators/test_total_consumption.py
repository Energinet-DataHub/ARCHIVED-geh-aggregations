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
from geh_stream.codelists import Names
from geh_stream.aggregation_utils.aggregators import calculate_total_consumption
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
import pytest
import pandas as pd
from geh_stream.codelists import Quality


@pytest.fixture(scope="module")
def net_exchange_schema():
    return StructType() \
        .add(Names.grid_area.value, StringType(), False) \
        .add(Names.time_window.value,
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add("in_sum", DecimalType(20, 1)) \
        .add("out_sum", DecimalType(20, 1)) \
        .add(Names.sum_quantity.value, DecimalType(20, 1)) \
        .add(Names.aggregated_quality.value, StringType())


@pytest.fixture(scope="module")
def agg_net_exchange_factory(spark, net_exchange_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Names.grid_area.value: ["1", "1", "1", "1", "1", "2"],
            Names.time_window.value: [
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 1, 0), "end": datetime(2020, 1, 1, 2, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
            ],
            "in_sum": [Decimal(2.0), Decimal(2.0), Decimal(2.0), Decimal(2.0), Decimal(2.0), Decimal(2.0)],
            "out_sum": [Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0)],
            Names.sum_quantity.value: [Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0)],
            Names.aggregated_quality.value: ["56", "56", "56", "56", "QM", "56"]
        })

        return spark.createDataFrame(pandas_df, schema=net_exchange_schema)
    return factory


@pytest.fixture(scope="module")
def production_schema():
    return StructType() \
        .add(Names.grid_area.value, StringType(), False) \
        .add(Names.time_window.value,
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add(Names.sum_quantity.value, DecimalType(20, 1)) \
        .add(Names.aggregated_quality.value, StringType())


@pytest.fixture(scope="module")
def agg_production_factory(spark, production_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Names.grid_area.value: ["1", "1", "1", "1", "1", "2"],
            Names.time_window.value: [
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 1, 0), "end": datetime(2020, 1, 1, 2, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
            ],
            Names.sum_quantity.value: [Decimal(1.0), Decimal(2.0), Decimal(3.0), Decimal(4.0), Decimal(5.0), Decimal(6.0)],
            Names.aggregated_quality.value: ["56", "56", "56", "56", "E01", "56"]
        })

        return spark.createDataFrame(pandas_df, schema=production_schema)
    return factory


@pytest.fixture(scope="module")
def agg_total_production_factory(spark, production_schema):
    def factory(quality):
        pandas_df = pd.DataFrame({
            Names.grid_area.value: [],
            Names.time_window.value: [],
            Names.sum_quantity.value: [],
            Names.aggregated_quality.value: []})

        pandas_df = pandas_df.append({
            Names.grid_area.value: "1",
            Names.time_window.value: {
                "start": datetime(2020, 1, 1, 0, 0),
                "end": datetime(2020, 1, 1, 1, 0)
                           },
            Names.sum_quantity.value: Decimal(1.0),
            Names.aggregated_quality.value: quality
        }, ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=production_schema)
    return factory


@pytest.fixture(scope="module")
def agg_total_net_exchange_factory(spark, net_exchange_schema):
    def factory(quality):
        pandas_df = pd.DataFrame({
            Names.grid_area.value: [],
            Names.time_window.value: [],
            "in_sum": [],
            "out_sum": [],
            Names.sum_quantity.value: [],
            Names.aggregated_quality.value: []
        })

        pandas_df = pandas_df.append({
            Names.grid_area.value: "1",
            Names.time_window.value: {
                "start": datetime(2020, 1, 1, 0, 0),
                "end": datetime(2020, 1, 1, 1, 0)
                },
            "in_sum": Decimal(1.0),
            "out_sum": Decimal(1.0),
            Names.sum_quantity.value: Decimal(1.0),
            Names.aggregated_quality.value: quality
        }, ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=net_exchange_schema)
    return factory


def test_grid_area_total_consumption(agg_net_exchange_factory, agg_production_factory):
    net_exchange_df = agg_net_exchange_factory()
    production_df = agg_production_factory()
    aggregated_df = calculate_total_consumption(net_exchange_df, production_df)

    assert aggregated_df.collect()[0][Names.sum_quantity.value] == Decimal("14.0") and \
        aggregated_df.collect()[1][Names.sum_quantity.value] == Decimal("6.0") and \
        aggregated_df.collect()[2][Names.sum_quantity.value] == Decimal("7.0")


@pytest.mark.parametrize("prod_quality, ex_quality, expected_quality", [
                        (Quality.estimated.value, Quality.estimated.value, Quality.estimated.value),
                        (Quality.estimated.value, Quality.quantity_missing.value, Quality.estimated.value),
                        (Quality.estimated.value, Quality.as_read.value, Quality.estimated.value),
                        (Quality.quantity_missing.value, Quality.quantity_missing.value, Quality.estimated.value),
                        (Quality.quantity_missing.value, Quality.as_read.value, Quality.estimated.value),
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

    assert result_df.collect()[0][Names.aggregated_quality.value] == expected_quality
