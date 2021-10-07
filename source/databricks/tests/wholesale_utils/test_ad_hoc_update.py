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
from pyspark.sql.types import StructType, StringType, StructField, TimestampType
from pyspark.sql.functions import col, lit, when

# Integration event schemas
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
    StructField("effective_date", TimestampType(), False),
])

settlement_method_updated_schema = StructType([
    StructField("metering_point_id", StringType(), False),
    StructField("settlement_method", StringType(), False),
    StructField("effective_date", TimestampType(), False),
])

# Domain model schemas
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
    create_consumption_mp_event = [("1", "E17", "1234", "500", "D01", "D01", "P1H", "NSG1", "23", datetime(2021, 1, 1, 0, 0))]

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


def test_settlement_method_changed(spark):
    consumption_mp = [("1", "E17", "1234", "P1H", "kwh", "23", "D01", datetime(2021, 1, 1, 0, 0), datetime(9999, 1, 1))]
    consumption_mp_df = spark.createDataFrame(consumption_mp, schema=metering_point_base_schema)

    settlement_method_updated_event = [("1", "D02", datetime(2021, 1, 7, 0, 0))]
    settlement_method_updated_df = spark.createDataFrame(settlement_method_updated_event, schema=settlement_method_updated_schema)

    # Logic to find dataframe
    update_func = (when((col("valid_from") <= col("effective_date")) & (col("valid_to") > col("effective_date")), col("effective_date"))
                   .otherwise(col("valid_to")))

    settlement_method_updated_df = settlement_method_updated_df.withColumnRenamed("settlement_method", "updated_settlement_method")

    existing_periods_df = consumption_mp_df.join(settlement_method_updated_df, "metering_point_id", "inner") \
        .withColumn("valid_to", update_func)

    existing_periods_df.show()

    dataframe_to_add = existing_periods_df.filter(col("valid_to") == col("effective_date"))

    # Updated dataframe to add
    dataframe_to_add = dataframe_to_add \
        .withColumn("settlement_method", col("updated_settlement_method")) \
        .withColumn("valid_from", col("effective_date")) \
        .withColumn("valid_to", lit(datetime(9999, 1, 1)).cast("timestamp"))

    resulting_dataframe_period_df = existing_periods_df.union(dataframe_to_add)

    result_df = resulting_dataframe_period_df.select("metering_point_id", "metering_point_type", "parent_id", "resolution", "unit", "product", "settlement_method", "valid_from", "valid_to")

    consumption_mp_df.show()
    settlement_method_updated_df.show()
    result_df.show()


def test_settlement_method_changed_back_in_time_advanced_dataset_multiple_short_periods(spark):
    consumption_mps = [
        ("1", "E17", "1234", "P1H", "kwh", "23", "D01", datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 7, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D02", datetime(2021, 1, 7, 0, 0), datetime(2021, 1, 9, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D03", datetime(2021, 1, 9, 0, 0), datetime(2021, 1, 12, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D04", datetime(2021, 1, 12, 0, 0), datetime(2021, 1, 17, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D05", datetime(2021, 1, 17, 0, 0), datetime(9999, 1, 1, 0, 0))]

    consumption_mps_df = spark.createDataFrame(consumption_mps, schema=metering_point_base_schema)

    settlement_method_updated_event = [("1", "D06", datetime(2021, 1, 8, 0, 0))]
    settlement_method_updated_df = spark.createDataFrame(settlement_method_updated_event, schema=settlement_method_updated_schema)

    # Logic to find and update valid_to on dataframe
    update_func_valid_to = (when((col("valid_from") <= col("effective_date")) & (col("valid_to") > col("effective_date")), col("effective_date"))
                            .otherwise(col("valid_to")))

    settlement_method_updated_df = settlement_method_updated_df.withColumnRenamed("settlement_method", "updated_settlement_method")

    existing_periods_df = consumption_mps_df.join(settlement_method_updated_df, "metering_point_id", "inner") \
        .withColumn("valid_to", update_func_valid_to)

    existing_periods_df.show()

    row_to_add = existing_periods_df \
        .filter(col("valid_from") >= col("effective_date")) \
        .orderBy(col("valid_from")) \
        .first()

    print(row_to_add)
    rdd = spark.sparkContext.parallelize([row_to_add])

    schema_with_updates = StructType([
        StructField("metering_point_id", StringType(), False),
        StructField("metering_point_type", StringType(), False),
        StructField("parent_id", StringType(), False),
        StructField("resolution", StringType(), False),
        StructField("unit", StringType(), False),
        StructField("product", StringType(), False),
        StructField("settlement_method", StringType(), False),
        StructField("valid_from", TimestampType(), False),
        StructField("valid_to", TimestampType(), False),
        StructField("updated_settlement_method", StringType(), False),
        StructField("effective_date", TimestampType(), False),
    ])

    dataframe_to_add = spark.createDataFrame(rdd, schema=schema_with_updates)

    # Updated dataframe to add
    dataframe_to_add = dataframe_to_add \
        .withColumn("settlement_method", col("updated_settlement_method")) \
        .withColumn("valid_to", col("valid_from")) \
        .withColumn("valid_from", col("effective_date"))

    resulting_dataframe_period_df = existing_periods_df.union(dataframe_to_add)

    result_df = resulting_dataframe_period_df.select("metering_point_id", "metering_point_type", "parent_id", "resolution", "unit", "product", "settlement_method", "valid_from", "valid_to")

    consumption_mps_df.show()
    settlement_method_updated_df.show()
    result_df.orderBy(col("valid_from")).show()


def test_settlement_method_changed_back_in_time_existing_period(spark):
    consumption_mps = [
        ("1", "E17", "1234", "P1H", "kwh", "23", "D01", datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 7, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D02", datetime(2021, 1, 7, 0, 0), datetime(2021, 1, 9, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D03", datetime(2021, 1, 9, 0, 0), datetime(2021, 1, 12, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D04", datetime(2021, 1, 12, 0, 0), datetime(2021, 1, 17, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D05", datetime(2021, 1, 17, 0, 0), datetime(9999, 1, 1, 0, 0))]

    consumption_mps_df = spark.createDataFrame(consumption_mps, schema=metering_point_base_schema)

    settlement_method_updated_event = [("1", "D06", datetime(2021, 1, 9, 0, 0))]
    settlement_method_updated_df = spark.createDataFrame(settlement_method_updated_event, schema=settlement_method_updated_schema)

    settlement_method_updated_df = settlement_method_updated_df.withColumnRenamed("settlement_method", "updated_settlement_method")

    existing_periods_df = consumption_mps_df.join(settlement_method_updated_df, "metering_point_id", "inner") \
        .withColumn("settlement_method", when(col("valid_from") == col("effective_date"), col("updated_settlement_method")).otherwise(col("settlement_method")))

    existing_periods_df.show()

    result_df = existing_periods_df.select("metering_point_id", "metering_point_type", "parent_id", "resolution", "unit", "product", "settlement_method", "valid_from", "valid_to")

    consumption_mps_df.show()
    settlement_method_updated_df.show()
    result_df.orderBy(col("valid_from")).show()


def test_settlement_method_changed_in_future(spark):
    consumption_mps = [
        ("1", "E17", "1234", "P1H", "kwh", "23", "D01", datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 7, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D02", datetime(2021, 1, 7, 0, 0), datetime(2021, 1, 9, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D03", datetime(2021, 1, 9, 0, 0), datetime(2021, 1, 12, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D04", datetime(2021, 1, 12, 0, 0), datetime(2021, 1, 17, 0, 0)),
        ("1", "E17", "1234", "P1H", "kwh", "23", "D05", datetime(2021, 1, 17, 0, 0), datetime(9999, 1, 1, 0, 0))]

    consumption_mps_df = spark.createDataFrame(consumption_mps, schema=metering_point_base_schema)

    settlement_method_updated_event = [("1", "D07", datetime(2021, 1, 27, 0, 0))]
    settlement_method_updated_df = spark.createDataFrame(settlement_method_updated_event, schema=settlement_method_updated_schema)

    # Logic to find and update valid_to on dataframe
    update_func_valid_to = (when((col("effective_date") > col("valid_from")) & (col("effective_date") < col("valid_to")), col("effective_date"))
                            .otherwise(col("valid_to")))

    settlement_method_updated_df = settlement_method_updated_df.withColumnRenamed("settlement_method", "updated_settlement_method")

    existing_periods_df = consumption_mps_df.join(settlement_method_updated_df, "metering_point_id", "inner") \
        .withColumn("valid_to", update_func_valid_to)

    existing_periods_df.show()

    row_to_add = existing_periods_df \
        .filter(col("valid_to") == col("effective_date")) \
        .first()

    print(row_to_add)
    rdd = spark.sparkContext.parallelize([row_to_add])

    schema_with_updates = StructType([
        StructField("metering_point_id", StringType(), False),
        StructField("metering_point_type", StringType(), False),
        StructField("parent_id", StringType(), False),
        StructField("resolution", StringType(), False),
        StructField("unit", StringType(), False),
        StructField("product", StringType(), False),
        StructField("settlement_method", StringType(), False),
        StructField("valid_from", TimestampType(), False),
        StructField("valid_to", TimestampType(), False),
        StructField("updated_settlement_method", StringType(), False),
        StructField("effective_date", TimestampType(), False),
    ])

    dataframe_to_add = spark.createDataFrame(rdd, schema=schema_with_updates)

    # Updated dataframe to add
    dataframe_to_add = dataframe_to_add \
        .withColumn("settlement_method", col("updated_settlement_method")) \
        .withColumn("valid_from", col("effective_date")) \
        .withColumn("valid_to", lit(datetime(9999, 1, 1)).cast("timestamp"))

    resulting_dataframe_period_df = existing_periods_df.union(dataframe_to_add)

    result_df = resulting_dataframe_period_df.select("metering_point_id", "metering_point_type", "parent_id", "resolution", "unit", "product", "settlement_method", "valid_from", "valid_to")

    consumption_mps_df.show()
    settlement_method_updated_df.show()
    result_df.orderBy(col("valid_from")).show()
