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
from geh_stream.aggregation_utils.aggregators import calculate_added_system_correction
from geh_stream.codelists import Quality
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql.functions import col
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def grid_loss_schema():
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.time_window,
             StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.grid_loss, DecimalType(18, 3)) \
        .add(Colname.aggregated_quality, StringType())


@pytest.fixture(scope="module")
def agg_result_factory(spark, grid_loss_schema):
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: [],
            Colname.time_window: [],
            Colname.grid_loss: [],
        })
        pandas_df = pandas_df.append([{
            Colname.grid_area: str(1), Colname.time_window: {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}, Colname.grid_loss: Decimal(-12.567), Colname.aggregated_quality: Quality.estimated.value}, {
            Colname.grid_area: str(2), Colname.time_window: {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}, Colname.grid_loss: Decimal(34.32), Colname.aggregated_quality: Quality.estimated.value}, {
            Colname.grid_area: str(3), Colname.time_window: {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}, Colname.grid_loss: Decimal(0.0), Colname.aggregated_quality: Quality.estimated.value}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=grid_loss_schema)
    return factory


def test_added_system_correction_has_no_values_below_zero(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_system_correction(df)

    assert result.filter(col(Colname.added_system_correction) < 0).count() == 0


def test_added_system_correction_change_negative_value_to_positive(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_system_correction(df)

    assert result.collect()[0][Colname.added_system_correction] == Decimal("12.56700")


def test_added_system_correction_change_positive_value_to_zero(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_system_correction(df)

    assert result.collect()[1][Colname.added_system_correction] == Decimal("0.00000")


def test_added_system_correction_values_that_are_zero_stay_zero(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_system_correction(df)

    assert result.collect()[2][Colname.added_system_correction] == Decimal("0.00000")
