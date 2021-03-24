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
from geh_stream.aggregation_utils.aggregators import calculate_added_system_correction
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql.functions import col
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def grid_loss_schema():
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("time_window",
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add("grid_loss", DecimalType(18, 5))


@pytest.fixture(scope="module")
def agg_result_factory(spark, grid_loss_schema):
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """
    def factory():
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": [],
            "time_window": [],
            "grid_loss": [],
        })
        pandas_df = pandas_df.append([{
            "MeteringGridArea_Domain_mRID": str(1), "time_window": {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}, "grid_loss": Decimal(-12.567), }, {
            "MeteringGridArea_Domain_mRID": str(2), "time_window": {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}, "grid_loss": Decimal(34.32), }, {
            "MeteringGridArea_Domain_mRID": str(3), "time_window": {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}, "grid_loss": Decimal(0.0), }],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=grid_loss_schema)
    return factory


def test_added_system_correction_has_no_values_below_zero(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_system_correction(df)

    assert result.filter(col("added_system_correction") < 0).count() == 0


def test_added_system_correction_change_negative_value_to_positive(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_system_correction(df)

    assert result.collect()[0]["added_system_correction"] == Decimal("12.56700")


def test_added_system_correction_change_positive_value_to_zero(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_system_correction(df)

    assert result.collect()[1]["added_system_correction"] == Decimal("0.00000")


def test_added_system_correction_values_that_are_zero_stay_zero(agg_result_factory):
    df = agg_result_factory()

    result = calculate_added_system_correction(df)

    assert result.collect()[2]["added_system_correction"] == Decimal("0.00000")
