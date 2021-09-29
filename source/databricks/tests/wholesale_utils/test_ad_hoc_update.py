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
import pytest
import pandas as pd
from pyspark.sql.types import StructType, StringType, StructField, TimestampType
from pyspark.sql.functions import col, lit, to_timestamp


consumption_metering_point_created_event_schema = StructType([
    StructField("metering_point_id", StringType(), False),
    StructField("metering_point_type", StringType(), False),
    StructField("metering_gsrn_number", StringType(), False),
    StructField("metering_grid_area", StringType(), False),
    StructField("settlement_method", StringType(), False),
    StructField("metering_method", StringType(), False),
    StructField("meter_reading_periodicity", StringType(), False),
    StructField("net_settlement_group", StringType(), False),
    StructField("product", StringType(), False),
    StructField("effective_date", TimestampType(), False)
])


metering_point_base_schema = StructType([
    StructField("metering_point_id", StringType(), False),
    StructField("metering_point_type", StringType(), False),
    StructField("parent_id", StringType(), False),
    StructField("resolution", StringType(), False),
    StructField("unit", StringType(), False),
    StructField("product", StringType(), False),
    StructField("settlement_method", StringType(), False),
    StructField("valid_from", TimestampType(), False),
    StructField("valid_to", TimestampType(), False),
])


metering_point_grid_area_schema = StructType([
    StructField("metering_point_id", StringType(), False),
    StructField("grid_area", StringType(), False),
    StructField("in_grid_area", StringType(), False),
    StructField("out_grid_area", StringType(), False),
    StructField("valid_from", TimestampType(), False),
    StructField("valid_to", TimestampType(), False),
])


metering_point_connection_state_schema = StructType([
    StructField("metering_point_id", StringType(), False),
    StructField("connection_state", StringType(), False),
    StructField("valid_from", TimestampType(), False),
    StructField("valid_to", TimestampType(), False),
])


def test_create_consumption_metering_point(spark):
    create_consumption_mp_event = [("1", "E17", "1234", "500", "D01", "D01", "P1H", "NVM", "23", datetime(2021, 1, 1, 0, 0))]

    consumption_metering_point_event_df = spark.createDataFrame(create_consumption_mp_event, schema=consumption_metering_point_created_event_schema)

    metering_point_base_df = consumption_metering_point_event_df \
        .select("metering_point_id", "metering_point_type", "settlement_method", "meter_reading_periodicity", "product", "effective_date")

    metering_point_base_df = metering_point_base_df \
        .withColumn("parent_id", lit("")) \
        .withColumn("unit", lit("kwh")) \
        .withColumn("valid_to", lit(datetime(9999, 1, 1)).cast("timestamp")) \
        .withColumnRenamed("meter_reading_periodicity", "resolution") \
        .withColumnRenamed("effective_date", "valid_from")

    metering_point_grid_area_df = consumption_metering_point_event_df \
        .select("metering_point_id", "metering_grid_area", "effective_date")

    metering_point_grid_area_df = metering_point_grid_area_df \
        .withColumn("in_grid_area", lit("")) \
        .withColumn("out_grid_area", lit("")) \
        .withColumn("valid_to", lit(datetime(9999, 1, 1)).cast("timestamp")) \
        .withColumnRenamed("effective_date", "valid_from")

    metering_point_connection_state_df = consumption_metering_point_event_df \
        .select("metering_point_id", "effective_date")

    metering_point_connection_state_df = metering_point_connection_state_df \
        .withColumn("connection_state", lit("")) \
        .withColumn("valid_to", lit(datetime(9999, 1, 1)).cast("timestamp")) \
        .withColumnRenamed("effective_date", "valid_from")

    print(consumption_metering_point_event_df.show())
    print(metering_point_base_df.show())
    print(metering_point_grid_area_df.show())
    print(metering_point_connection_state_df.show())

    assert metering_point_base_df.collect()[0]["metering_point_id"] == "1"
    # assert metering_point_grid_area.filter(col("metering_point_id") == "1")
    # assert metering_point_connection_state.filter(col("metering_point_id") == "1")
