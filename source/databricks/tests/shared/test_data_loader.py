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
from datetime import datetime
from geh_stream.shared.data_loader import select_latest_point_data, filter_time_series_by_metering_points
from tests.helpers.dataframe_creators.time_series_creator import time_series_factory
from tests.helpers.dataframe_creators.metering_point_creator import metering_point_factory
from pyspark.sql.types import StructType, TimestampType
from typing import List
import pytest
import pandas as pd


def test_select_latest_point_data(time_series_factory):
    # Arrange
    time_series_df_1 = time_series_factory(datetime(2020, 1, 1, 0, 0), registration_time=datetime(2020, 1, 1, 0, 0))
    time_series_df_2 = time_series_factory(datetime(2020, 1, 2, 0, 0), registration_time=datetime(2020, 1, 2, 0, 0))
    time_series_df_3 = time_series_factory(datetime(2020, 1, 1, 0, 0), registration_time=datetime(2020, 1, 3, 0, 0))
    time_series_df = time_series_df_1.union(time_series_df_2).union(time_series_df_3)
    expected = time_series_df_2.union(time_series_df_3)

    # Act
    df = select_latest_point_data(time_series_df)

    # Assert
    assert df.collect() == expected.collect()


def test_filter_time_series_by_metering_points(time_series_factory, metering_point_factory):
    # Arrange
    time_series_df_1 = time_series_factory(datetime(2020, 1, 1, 0, 0), metering_point_id="D01")
    time_series_df_2 = time_series_factory(datetime(2020, 1, 2, 0, 0), metering_point_id="D02")
    time_series_df = time_series_df_1.union(time_series_df_2)
    metering_point_df = metering_point_factory(datetime(2020, 1, 1, 0, 0), datetime(2020, 1, 2, 0, 0), metering_point_id="D02")

    # Act
    df = filter_time_series_by_metering_points(time_series_df, metering_point_df)

    # Assert
    assert df.collect() == time_series_df_2.collect()
