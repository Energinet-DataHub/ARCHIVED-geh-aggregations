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
from geh_stream.aggregation_utils.aggregators import calculate_added_grid_loss
from geh_stream.codelists import Quality
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql.functions import col
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def grid_loss_schema():
    return StructType() \
        .add(Names.grid_area.value, StringType(), False) \
        .add(Names.time_window.value,
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add(Names.grid_loss.value, DecimalType(18, 5)) \
        .add(Names.aggregated_quality.value, StringType())


@pytest.fixture(scope="module")
def expected_schema():
    return StructType() \
        .add(Names.grid_area.value, StringType(), False) \
        .add(Names.time_window.value,
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add(Names.added_grid_loss.value, DecimalType(18, 5))


@pytest.fixture(scope="module")
def agg_result_factory(spark, grid_loss_schema):
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """
    def factory():
        pandas_df = pd.DataFrame({
            Names.grid_area.value: [],
            Names.time_window.value: [],
            Names.grid_loss.value: [],
            Names.aggregated_quality.value: []
        })
        pandas_df = pandas_df.append([{
            Names.grid_area.value: str(1), Names.time_window.value: {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}, Names.grid_loss.value: Decimal(-12.567), Names.aggregated_quality.value: Quality.estimated.value}, {
            Names.grid_area.value: str(2), Names.time_window.value: {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}, Names.grid_loss.value: Decimal(34.32), Names.aggregated_quality.value: Quality.estimated.value}, {
            Names.grid_area.value: str(3), Names.time_window.value: {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}, Names.grid_loss.value: Decimal(0.0), Names.aggregated_quality.value: Quality.estimated.value}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=grid_loss_schema)
    return factory


def test_grid_area_grid_loss_has_no_values_below_zero(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_grid_loss(df)

    assert result.filter(col(Names.added_grid_loss.value) < 0).count() == 0


def test_grid_area_grid_loss_changes_negative_values_to_zero(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_grid_loss(df)

    assert result.collect()[0][Names.added_grid_loss.value] == Decimal("0.00000")


def test_grid_area_grid_loss_positive_values_will_not_change(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_grid_loss(df)

    assert result.collect()[1][Names.added_grid_loss.value] == Decimal("34.32000")


def test_grid_area_grid_loss_values_that_are_zero_stay_zero(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_grid_loss(df)

    assert result.collect()[2][Names.added_grid_loss.value] == Decimal("0.00000")


def test_returns_correct_schema(agg_result_factory, expected_schema):
    """
    Aggregator should return the correct schema, including the proper fields for the aggregated quantity values
    and time window (from the single-hour resolution specified in the aggregator).
    """
    df = agg_result_factory()
    aggregated_df = calculate_added_grid_loss(df)
    assert aggregated_df.schema == expected_schema
