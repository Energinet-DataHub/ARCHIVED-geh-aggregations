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

from geh_stream.event_dispatch.meteringpoint_dispatcher import handle_update as method_to_test
from datetime import datetime
import pytest
from pyspark.sql.types import StructType, StringType, StructField, TimestampType
from pyspark.sql.functions import col, lit, when

metering_point_base_schema = StructType([
        StructField("metering_point_id", StringType(), False),
        StructField("metering_point_type", StringType(), False),
        StructField("gsrn_number", StringType(), False),
        StructField("grid_area_code", StringType(), False),
        StructField("settlement_method", StringType(), False),
        StructField("metering_method", StringType(), False),
        StructField("meter_reading_periodicity", StringType(), False),
        StructField("net_settlement_group", StringType(), False),
        StructField("product", StringType(), False),
        StructField("parent_id", StringType(), False),
        StructField("connection_state", StringType(), False),
        StructField("unit_type", StringType(), False),
        StructField("valid_from", TimestampType(), False),
        StructField("valid_to", TimestampType(), False),
    ])

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

def test_changed_period_after_update(spark):
    consumption_mps = [
        ("1", "E17", "1234", "500,", "D01", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 7, 0, 0)),
        ("1", "E17", "1234", "500,", "D02", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 7, 0, 0), datetime(2021, 1, 9, 0, 0)),
        ("1", "E17", "1234", "500,", "D03", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 9, 0, 0), datetime(2021, 1, 12, 0, 0)),
        ("1", "E17", "1234", "500,", "D04", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 12, 0, 0), datetime(2021, 1, 17, 0, 0)),
        ("1", "E17", "1234", "500,", "D05", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 17, 0, 0), datetime(9999, 1, 1, 0, 0))]

    consumption_mps_df = spark.createDataFrame(consumption_mps, schema=metering_point_base_schema)

    settlement_method_updated_event = [("1", "D06", datetime(2021, 1, 7, 0, 0))]
    settlement_method_updated_df = spark.createDataFrame(settlement_method_updated_event, schema=settlement_method_updated_schema)

    result_df = method_to_test(spark, consumption_mps_df, settlement_method_updated_df, ["settlement_method"]).orderBy("valid_to")

    assert(consumption_mps_df.count() == 5)
    assert(result_df.count() == 5)

    assert(result_df.collect()[0]["valid_from"] == datetime(2021, 1, 1, 0, 0))
    assert(result_df.collect()[0]["valid_to"] == datetime(2021, 1, 7, 0, 0))

    assert(result_df.collect()[1]["valid_from"] == datetime(2021, 1, 7, 0, 0))
    assert(result_df.collect()[1]["valid_to"] == datetime(2021, 1, 9, 0, 0))

    assert(result_df.collect()[2]["valid_from"] == datetime(2021, 1, 9, 0, 0))
    assert(result_df.collect()[2]["valid_to"] == datetime(2021, 1, 12, 0, 0))

    assert(result_df.collect()[3]["valid_from"] == datetime(2021, 1, 12, 0, 0))
    assert(result_df.collect()[3]["valid_to"] == datetime(2021, 1, 17, 0, 0))

    assert(result_df.collect()[0]["settlement_method"] == "D01") # 1/1
    assert(result_df.collect()[1]["settlement_method"] == "D06") # 7/1
    assert(result_df.collect()[2]["settlement_method"] == "D03") # 9/1
    assert(result_df.collect()[3]["settlement_method"] == "D04") # 12/1
    assert(result_df.collect()[4]["settlement_method"] == "D05") # 17/1


def test_add_new_period_after_update(spark):
    consumption_mps = [
        ("1", "E17", "1234", "500,", "D01", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 7, 0, 0)),
        ("1", "E17", "1234", "500,", "D02", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 7, 0, 0), datetime(2021, 1, 9, 0, 0)),
        ("1", "E17", "1234", "500,", "D03", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 9, 0, 0), datetime(2021, 1, 12, 0, 0)),
        ("1", "E17", "1234", "500,", "D04", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 12, 0, 0), datetime(2021, 1, 17, 0, 0)),
        ("1", "E17", "1234", "500,", "D05", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 17, 0, 0), datetime(9999, 1, 1, 0, 0))]

    consumption_mps_df = spark.createDataFrame(consumption_mps, schema=metering_point_base_schema)

    settlement_method_updated_event = [("1", "D06", datetime(2021, 1, 8, 0, 0))]
    settlement_method_updated_df = spark.createDataFrame(settlement_method_updated_event, schema=settlement_method_updated_schema)

    result_df = method_to_test(spark, consumption_mps_df, settlement_method_updated_df, ["settlement_method"]).orderBy("valid_to")
    # print(result_df.show())

    assert(consumption_mps_df.count() == 5)
    assert(result_df.count() == 6)

    assert(result_df.collect()[0]["valid_from"] == datetime(2021, 1, 1, 0, 0))
    assert(result_df.collect()[0]["valid_to"] == datetime(2021, 1, 7, 0, 0))

    assert(result_df.collect()[1]["valid_from"] == datetime(2021, 1, 7, 0, 0))
    assert(result_df.collect()[1]["valid_to"] == datetime(2021, 1, 8, 0, 0))

    assert(result_df.collect()[2]["valid_from"] == datetime(2021, 1, 8, 0, 0))
    assert(result_df.collect()[2]["valid_to"] == datetime(2021, 1, 9, 0, 0))

    assert(result_df.collect()[3]["valid_from"] == datetime(2021, 1, 9, 0, 0))
    assert(result_df.collect()[3]["valid_to"] == datetime(2021, 1, 12, 0, 0))

    assert(result_df.collect()[0]["settlement_method"] == "D01") # 1/1
    assert(result_df.collect()[1]["settlement_method"] == "D02") # 7/1
    assert(result_df.collect()[2]["settlement_method"] == "D06") # 8/1
    assert(result_df.collect()[3]["settlement_method"] == "D03") # 9/1
    assert(result_df.collect()[4]["settlement_method"] == "D04") # 12/1
    assert(result_df.collect()[5]["settlement_method"] == "D05") # 17/1

def test_add_new_future_period_after_update(spark):
    consumption_mps = [
        ("1", "E17", "1234", "500,", "D01", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 1, 0, 0), datetime(2021, 1, 7, 0, 0)),
        ("1", "E17", "1234", "500,", "D02", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 7, 0, 0), datetime(2021, 1, 9, 0, 0)),
        ("1", "E17", "1234", "500,", "D03", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 9, 0, 0), datetime(2021, 1, 12, 0, 0)),
        ("1", "E17", "1234", "500,", "D04", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 12, 0, 0), datetime(2021, 1, 17, 0, 0)),
        ("1", "E17", "1234", "500,", "D05", "x", "x", "x", "x", "x", "x", "x", datetime(2021, 1, 17, 0, 0), datetime(9999, 1, 1, 0, 0))]

    consumption_mps_df = spark.createDataFrame(consumption_mps, schema=metering_point_base_schema)

    settlement_method_updated_event = [("1", "D06", datetime(2021, 1, 18, 0, 0))]
    settlement_method_updated_df = spark.createDataFrame(settlement_method_updated_event, schema=settlement_method_updated_schema)

    result_df = method_to_test(spark, consumption_mps_df, settlement_method_updated_df, ["settlement_method"]).orderBy("valid_to")

    assert(consumption_mps_df.count() == 5)
    assert(result_df.count() == 6)

    assert(result_df.collect()[0]["valid_from"] == datetime(2021, 1, 1, 0, 0))
    assert(result_df.collect()[0]["valid_to"] == datetime(2021, 1, 7, 0, 0))

    assert(result_df.collect()[1]["valid_from"] == datetime(2021, 1, 7, 0, 0))
    assert(result_df.collect()[1]["valid_to"] == datetime(2021, 1, 9, 0, 0))

    assert(result_df.collect()[2]["valid_from"] == datetime(2021, 1, 9, 0, 0))
    assert(result_df.collect()[2]["valid_to"] == datetime(2021, 1, 12, 0, 0))

    assert(result_df.collect()[3]["valid_from"] == datetime(2021, 1, 12, 0, 0))
    assert(result_df.collect()[3]["valid_to"] == datetime(2021, 1, 17, 0, 0))

    assert(result_df.collect()[4]["valid_from"] == datetime(2021, 1, 17, 0, 0))
    assert(result_df.collect()[4]["valid_to"] == datetime(2021, 1, 18, 0, 0))

    assert(result_df.collect()[5]["valid_from"] == datetime(2021, 1, 18, 0, 0))
    assert(result_df.collect()[5]["valid_to"] == datetime(9999, 1, 1, 0, 0))

    assert(result_df.collect()[0]["settlement_method"] == "D01") # 1/1
    assert(result_df.collect()[1]["settlement_method"] == "D02") # 7/1
    assert(result_df.collect()[2]["settlement_method"] == "D03") # 9/1
    assert(result_df.collect()[3]["settlement_method"] == "D04") # 12/1
    assert(result_df.collect()[4]["settlement_method"] == "D05") # 17/1
    assert(result_df.collect()[5]["settlement_method"] == "D06") # 18/1