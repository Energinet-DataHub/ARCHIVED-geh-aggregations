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

from geh_stream.event_dispatch import dispatcher_base
from geh_stream.codelists import Colname

from pyspark.sql.types import StringType, StructType, StructField, TimestampType
from datetime import datetime

periods_schema = StructType([
    StructField(Colname.from_date, TimestampType(), False),
    StructField(Colname.to_date, TimestampType(), False),
    StructField(Colname.effective_date, TimestampType(), False),
])

periods = [
    (datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 7, 0, 0), datetime(2021, 1, 12, 0, 0)),
    (datetime(2021, 1, 7, 0, 0), datetime(2021, 1, 9, 0, 0), datetime(2021, 1, 12, 0, 0)),
    (datetime(2021, 1, 9, 0, 0), datetime(2021, 1, 12, 0, 0), datetime(2021, 1, 12, 0, 0)),
    (datetime(2021, 1, 12, 0, 0), datetime(2021, 1, 17, 0, 0), datetime(2021, 1, 12, 0, 0)),
    (datetime(9999, 1, 17, 0, 0), datetime(9999, 1, 1, 0, 0), datetime(2021, 1, 12, 0, 0))]


def test__split_out_first_period__return_df_where_to_date_equals_effective_date(spark):

    # Arrange
    cols_to_select = [Colname.to_date, Colname.effective_date]
    df = spark.createDataFrame(periods, schema=periods_schema)

    # Act
    sut = dispatcher_base.__split_out_first_period(df, cols_to_select)

    # Assert
    assert sut.count() == 1
    assert sut.collect()[0][Colname.to_date] == sut.collect()[0][Colname.effective_date] == datetime(2021, 1, 12, 0, 0)


def test__get_periods_to_keep__return_periods_before_or_equal_to_effective_date(spark):

    # Arrange
    df = spark.createDataFrame(periods, schema=periods_schema)

    # Act
    sut = dispatcher_base.__get_periods_to_keep(df)

    # Assert
    assert sut.count() == 3


def test__get_periods_to_update__return_periods_after_effective_date(spark):

    # Arrange
    df = spark.createDataFrame(periods, schema=periods_schema)

    # Act
    sut = dispatcher_base.__get_periods_to_update(df)

    # Assert
    assert sut.count() == 2


def test__get_periods_to_update__return_periods_after_effective_date__orderby_from_date(spark):

    # Arrange
    df = spark.createDataFrame(periods, schema=periods_schema)

    # Act
    sut = dispatcher_base.__get_periods_to_update(df)

    # Assert
    assert sut.collect()[0][Colname.from_date] == datetime(2021, 1, 12, 0, 0)


def test__rename_event_columns_to_update__prefix_cols_to_change_with_updated(spark):

    # Arrange
    event = [("col1", "col2", "col3")]
    event_schema = StructType([
        StructField("col1", StringType(), False),
        StructField("col2", StringType(), False),
        StructField("col3", StringType(), False),
    ])

    event_df = spark.createDataFrame(event, schema=event_schema)

    cols_to_change = ["col1", "col2"]

    # Act
    sut = dispatcher_base.__rename_event_columns_to_update(event_df, cols_to_change)

    # Assert
    assert "updated_col1" in sut.columns
    assert "updated_col2" in sut.columns
    assert "col3" in sut.columns
    assert "col1" not in sut.columns
    assert "col2" not in sut.columns
