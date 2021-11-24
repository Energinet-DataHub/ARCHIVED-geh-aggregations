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

from pyspark.context import SparkContext
from pyspark.sql import DataFrame
from pyspark.sql.functions import col
from pyspark.sql.types import StructType, StructField, StringType, TimestampType
from datetime import datetime
import pytest


def test__foreach_partition(spark):
    event_schema = StructType([
          StructField("event_id", StringType(), True),
          StructField("event_name", StringType(), True),
          StructField("processed_date", TimestampType(), True),
          StructField("domain", StringType(), True),
          StructField("body", StringType(), True)
    ])

    events = [
        ("1", "ConsumptionMeteringPointCreated", datetime(2021, 10, 1, 12, 0, 0), "MeteringPoint", '{"metering_point_id":"1","metering_point_type":"E17","grid_area":"501","settlement_method":"D01","metering_method":"D01","resolution":"PT1H","product":"8716867000030","connection_state":"D03","unit":"KWH","effective_date":"2021-06-30T22:00:00Z","Transaction"{"mRID":"4e047bb2f15f4f828a07b411c190ec9f"}}'),
        ("2", "ConsumptionMeteringPointCreated", datetime(2021, 10, 1, 12, 1, 0), "MeteringPoint", '{"metering_point_id":"2","metering_point_type":"E17","grid_area":"501","settlement_method":"D01","metering_method":"D01","resolution":"PT1H","product":"8716867000030","connection_state":"D03","unit":"KWH","effective_date":"2021-06-30T22:00:00Z","Transaction"{"mRID":"4e047bb2f15f4f828a07b411c190ec9f"}}'),
        ("3", "ConsumptionMeteringPointCreated", datetime(2021, 10, 1, 12, 2, 0), "MeteringPoint", '{"metering_point_id":"3","metering_point_type":"E17","grid_area":"501","settlement_method":"D01","metering_method":"D01","resolution":"PT1H","product":"8716867000030","connection_state":"D03","unit":"KWH","effective_date":"2021-06-30T22:00:00Z","Transaction"{"mRID":"4e047bb2f15f4f828a07b411c190ec9f"}}'),
        ("4", "MeteringPointConnected", datetime(2021, 10, 1, 12, 3, 0), "MeteringPoint", '{"metering_point_id":"1","connection_state":"E22","effective_date":"2021-07-03T22:00:00Z","Transaction":{"mRID":"d6944df877aa48c2927ff1e215b594aa"}}'),
        ("5", "MeteringPointConnected", datetime(2021, 10, 1, 12, 4, 0), "MeteringPoint", '{"metering_point_id":"2","connection_state":"E22","effective_date":"2021-07-03T22:00:00Z","Transaction":{"mRID":"d6944df877aa48c2927ff1e215b594aa"}}'),
        ("6", "MeteringPointConnected", datetime(2021, 10, 1, 12, 5, 0), "MeteringPoint", '{"metering_point_id":"3","connection_state":"E22","effective_date":"2021-07-03T22:00:00Z","Transaction":{"mRID":"d6944df877aa48c2927ff1e215b594aa"}}'),
        ("7", "EnergySupplierUpdated", datetime(2021, 10, 1, 12, 6, 0), "MarketRole", '{"metering_point_id":"1","energy_supplier_id":"1","effective_date":"2021-07-02T22:00:00Z","Transaction":{"mRID":"d6944df877aa48c2927ff1e215b594aa"}}}'),
        ("8", "EnergySupplierUpdated", datetime(2021, 10, 1, 12, 7, 0), "MarketRole", '{"metering_point_id":"2","energy_supplier_id":"1","effective_date":"2021-07-02T22:00:00Z","Transaction":{"mRID":"d6944df877aa48c2927ff1e215b594aa"}}}'),
        ("9", "EnergySupplierUpdated", datetime(2021, 10, 1, 12, 8, 0), "MarketRole", '{"metering_point_id":"3","energy_supplier_id":"1","effective_date":"2021-07-02T22:00:00Z","Transaction":{"mRID":"d6944df877aa48c2927ff1e215b594aa"}}}')
    ]

    event_df = spark.createDataFrame(events, event_schema)
    event_df.show()

    event_df.foreachPartition(lambda partition: func(partition))


def func(partition):
    # spark = SparkContext.getOrCreate()
    # mp_schema = StructType([
    #       StructField("metering_point_id", StringType(), True)
    # ])

    # mr_schema = StructType([
    #       StructField("energy_supplier_id", StringType(), True)
    # ])

    # mp_df = spark.createDataFrame([], mp_schema)

    # mr_df = spark.createDataFrame([], mr_schema)

    # print("meteringpoint df")
    # mp_df.show()

    # print("marketrole df")
    # mr_df.show()

    for item in partition:
        print(item)
    # partition.foreach(lambda row: mutate(row))
    print(partition)


def mutate(row):
    # mp_df = mp_df.union(row.select("event_id as metering_point_id"))
    print("h")
