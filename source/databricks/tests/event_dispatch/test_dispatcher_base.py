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

from pyspark.sql.functions import col, when
from pyspark.sql.types import StringType, StructType, StructField, TimestampType
from datetime import datetime


periods_schema = StructType([
    StructField(Colname.metering_point_id, StringType(), False),
    StructField(Colname.from_date, TimestampType(), False),
    StructField(Colname.to_date, TimestampType(), False),
    StructField(Colname.effective_date, TimestampType(), False),
])

periods = [
    ("1", datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 7, 0, 0), datetime(2021, 1, 12, 0, 0)),
    ("1", datetime(2021, 1, 7, 0, 0), datetime(2021, 1, 9, 0, 0), datetime(2021, 1, 12, 0, 0)),
    ("1", datetime(2021, 1, 9, 0, 0), datetime(2021, 1, 12, 0, 0), datetime(2021, 1, 12, 0, 0)),
    ("1", datetime(2021, 1, 12, 0, 0), datetime(2021, 1, 17, 0, 0), datetime(2021, 1, 12, 0, 0)),
    ("1", datetime(9999, 1, 17, 0, 0), datetime(9999, 1, 1, 0, 0), datetime(2021, 1, 12, 0, 0))]


def test__update_single_period__set_to_date_to_effective_date_and_add_new_period(spark):

    # Arrange
    effective_date = datetime(2021, 1, 17, 0, 0)

    period = [
        ("1", datetime(2021, 1, 1, 0, 0), datetime(9999, 1, 1, 0, 0), effective_date)
    ]

    period_df = spark.createDataFrame(period, schema=periods_schema)

    update_func_to_date = col(Colname.effective_date)

    # Act
    sut = dispatcher_base.__update_single_period(period_df, update_func_to_date)

    # Assert
    assert sut.collect()[0][Colname.to_date] == effective_date
    assert sut.count() == 2


def test__update_multiple_periods__to_date_updated_and_new_period_added(spark):

    # Arrange
    effective_date = datetime(2021, 1, 10, 0, 0)

    periods = [
        ("1", datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 20, 0, 0), effective_date),
        ("1", datetime(2021, 1, 20, 0, 0), datetime(9999, 1, 1, 0, 0), effective_date),
    ]

    periods_df = spark.createDataFrame(periods, schema=periods_schema)

    update_func_to_date = (
        when(
            (col(Colname.from_date) < col(Colname.effective_date))
            & (col(Colname.to_date) > col(Colname.effective_date)),
            col(Colname.effective_date))
        .otherwise(col(Colname.to_date)))

    # Act
    sut = dispatcher_base.__update_multiple_periods(periods_df, update_func_to_date)
    result_df = sut.collect()

    # Assert
    assert result_df[0][Colname.to_date] == datetime(2021, 1, 10, 0, 0)
    assert result_df[1][Colname.to_date] == datetime(2021, 1, 20, 0, 0)
    assert result_df[2][Colname.to_date] == datetime(9999, 1, 1, 0, 0)


def test__split_out_first_period__return_df_where_to_date_equals_effective_date(spark):

    # Arrange
    cols_to_select = [Colname.to_date, Colname.effective_date]
    df = spark.createDataFrame(periods, schema=periods_schema)

    # Act
    sut = dispatcher_base.__split_out_first_period(df, cols_to_select)

    # Assert
    assert sut.count() == 1
    assert sut.collect()[0][Colname.to_date] == sut.collect()[0][Colname.effective_date] == datetime(2021, 1, 12, 0, 0)


def test__update_column_values__overwrite_col_to_change_with_updated_col_to_change(spark):

    # Arrange
    updated_periods = [("col1", "col2", "col3", "updated_col1", "updated_col2")]
    updated_periods_schema = StructType([
        StructField("col1", StringType(), False),
        StructField("col2", StringType(), False),
        StructField("col3", StringType(), False),
        StructField("updated_col1", StringType(), False),
        StructField("updated_col2", StringType(), False),
    ])

    updated_periods_df = spark.createDataFrame(updated_periods, schema=updated_periods_schema)

    cols_to_change = ["col1", "col2"]

    # Act
    sut = dispatcher_base.__update_column_values(cols_to_change, updated_periods_df)

    # Assert
    sut_collect = sut.collect()
    assert sut_collect[0]["col1"] == "updated_col1"
    assert sut_collect[0]["col2"] == "updated_col2"
    assert sut_collect[0]["col3"] == "col3"


def test__create_result__union_two_dataframes_and_return_df_with_multiple_columns(spark):

    # Arrange
    subset_1 = [
        (datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 7, 0, 0), "value")]

    subset_2 = [
        (datetime(2021, 1, 7, 0, 0), datetime(9999, 1, 1, 0, 0), "value")]

    subset_schema = StructType([
        StructField(Colname.from_date, TimestampType(), False),
        StructField(Colname.to_date, TimestampType(), False),
        StructField("col", StringType(), False),
    ])

    subset_1_df = spark.createDataFrame(subset_1, schema=subset_schema)
    subset_2_df = spark.createDataFrame(subset_2, schema=subset_schema)

    cols_to_select = [Colname.from_date, Colname.to_date, "col"]

    # Act
    result_df = dispatcher_base.__create_result(subset_1_df, subset_2_df, cols_to_select)

    # Assert
    assert result_df.count() == 2
    assert Colname.from_date in result_df.columns
    assert Colname.to_date in result_df.columns
    assert "col" in result_df.columns


def test__create_result__union_two_dataframes_and_return_df_with_one_column(spark):

    # Arrange
    subset_1 = [
        (datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 7, 0, 0), "value")]

    subset_2 = [
        (datetime(2021, 1, 7, 0, 0), datetime(9999, 1, 1, 0, 0), "value")]

    subset_schema = StructType([
        StructField(Colname.from_date, TimestampType(), False),
        StructField(Colname.to_date, TimestampType(), False),
        StructField("col", StringType(), False),
    ])

    subset_1_df = spark.createDataFrame(subset_1, schema=subset_schema)
    subset_2_df = spark.createDataFrame(subset_2, schema=subset_schema)

    cols_to_select = ["col"]

    # Act
    result_df = dispatcher_base.__create_result(subset_1_df, subset_2_df, cols_to_select)

    # Assert
    assert result_df.count() == 2
    assert Colname.from_date not in result_df.columns
    assert Colname.to_date not in result_df.columns
    assert "col" in result_df.columns


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
