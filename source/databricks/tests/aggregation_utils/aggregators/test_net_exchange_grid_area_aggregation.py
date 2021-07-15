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
import pytest
from decimal import Decimal
import pandas as pd
from datetime import datetime, timedelta
from geh_stream.codelists import Names
from geh_stream.aggregation_utils.aggregators import aggregate_net_exchange_per_ga
from geh_stream.codelists import MarketEvaluationPointType, ConnectionState, Quality
from pyspark.sql import DataFrame
import pyspark.sql.functions as F
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType


e_20 = MarketEvaluationPointType.exchange.value
date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string)
numberOfTestHours = 24

# Time series schema


@pytest.fixture(scope="module")
def time_series_schema():
    return StructType() \
        .add(Names.metering_point_type.value, StringType(), False) \
        .add(Names.in_grid_area.value, StringType()) \
        .add(Names.out_grid_area.value, StringType(), False) \
        .add(Names.quantity.value, DecimalType(38, 10)) \
        .add(Names.time.value, TimestampType()) \
        .add(Names.connection_state.value, StringType()) \
        .add(Names.aggregated_quality.value, StringType())


@pytest.fixture(scope="module")
def expected_schema():
    """
    Expected exchange aggregation output schema
    """
    return StructType() \
        .add(Names.grid_area.value, StringType()) \
        .add(Names.time_window.value,
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType())
             ) \
        .add(Names.sum_quantity.value, DecimalType(38, 9)) \
        .add(Names.aggregated_quality.value, StringType())


@pytest.fixture(scope="module")
def time_series_data_frame(spark, time_series_schema):
    """
    Sample Time Series DataFrame
    """
    # Create empty pandas df
    pandas_df = pd.DataFrame({
        Names.metering_point_type.value: [],
        Names.in_grid_area.value: [],
        Names.out_grid_area.value: [],
        Names.quantity.value: [],
        Names.time.value: [],
        Names.connection_state.value: [],
        Names.aggregated_quality.value: []
    })

    # add 24 hours of exchange with different examples of exchange between grid areas. See readme.md for more info

    for x in range(numberOfTestHours):
        pandas_df = add_row_of_data(pandas_df, e_20, "B", "A", Decimal(2) * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)

        pandas_df = add_row_of_data(pandas_df, e_20, "B", "A", Decimal("0.5") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)
        pandas_df = add_row_of_data(pandas_df, e_20, "B", "A", Decimal("0.7") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)

        pandas_df = add_row_of_data(pandas_df, e_20, "A", "B", Decimal(3) * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)
        pandas_df = add_row_of_data(pandas_df, e_20, "A", "B", Decimal("0.9") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)
        pandas_df = add_row_of_data(pandas_df, e_20, "A", "B", Decimal("1.2") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)

        pandas_df = add_row_of_data(pandas_df, e_20, "C", "A", Decimal("0.7") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)
        pandas_df = add_row_of_data(pandas_df, e_20, "A", "C", Decimal("1.1") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)
        pandas_df = add_row_of_data(pandas_df, e_20, "A", "C", Decimal("1.5") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)

    return spark.createDataFrame(pandas_df, schema=time_series_schema)


def add_row_of_data(pandas_df: pd.DataFrame, point_type, in_domain, out_domain, quantity: Decimal, timestamp, connectionState):
    """
    Helper method to create a new row in the dataframe to improve readability and maintainability
    """
    new_row = {
        Names.metering_point_type.value: point_type,
        Names.in_grid_area.value: in_domain,
        Names.out_grid_area.value: out_domain,
        Names.quantity.value: quantity,
        Names.time.value: timestamp,
        Names.connection_state.value: connectionState,
        Names.aggregated_quality.value: Quality.estimated.value
    }
    return pandas_df.append(new_row, ignore_index=True)


@pytest.fixture(scope="module")
def aggregated_data_frame(time_series_data_frame):
    """Perform aggregation"""
    return aggregate_net_exchange_per_ga(time_series_data_frame)


def test_test_data_has_correct_row_count(time_series_data_frame):
    """ Check sample data row count"""
    assert time_series_data_frame.count() == (9 * numberOfTestHours)


def test_exchange_aggregator_returns_correct_schema(aggregated_data_frame, expected_schema):
    """Check aggregation schema"""
    assert aggregated_data_frame.schema == expected_schema


def test_exchange_aggregator_returns_correct_aggregations(aggregated_data_frame):
    """Check accuracy of aggregations"""

    for x in range(numberOfTestHours):
        check_aggregation_row(aggregated_data_frame, "A", Decimal("3.8") * x, default_obs_time + timedelta(hours=x))
        check_aggregation_row(aggregated_data_frame, "B", Decimal("-1.9") * x, default_obs_time + timedelta(hours=x))
        check_aggregation_row(aggregated_data_frame, "C", Decimal("-1.9") * x, default_obs_time + timedelta(hours=x))


def check_aggregation_row(df: DataFrame, MeteringGridArea_Domain_mRID: str, sum: Decimal, time: datetime):
    """Helper function that checks column values for the given row"""
    gridfiltered = df.filter(df[Names.grid_area.value] == MeteringGridArea_Domain_mRID).select(F.col(Names.grid_area.value), F.col(
        Names.sum_quantity.value), F.col("{0}.start".format(Names.time_window.value)).alias("start"), F.col("{0}.end".format(Names.time_window.value)).alias("end"))
    res = gridfiltered.filter(gridfiltered["start"] == time).toPandas()
    assert res[Names.sum_quantity.value][0] == sum
