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
import configargparse
from pyspark import SparkConf
from pyspark.sql.session import SparkSession

# .option("checkpointLocation", checkpoint_path) \
# from pyspark.sql import DataFrame
# from pyspark.sql.functions import col, from_json, explode
# from pyspark.sql.types import DecimalType, StructType, StructField, StringType, TimestampType, ArrayType, BinaryType, IntegerType
# import json
# import datetime
# from delta.tables import *

# connectionString = "Endpoint=sb://evhnm-aggregation-aggregations-endk-u.servicebus.windows.net/;SharedAccessKeyName=evhar-aggregation-listener;SharedAccessKey=65Pfzom3sMCgStfORF+PlVzbMWxFasZaqXR+uWJCc/Q=;EntityPath=evh-aggregation"
# conf = {}
# conf["eventhubs.connectionString"] = sc._jvm.org.apache.spark.eventhubs.EventHubsUtils.encrypt(connectionString)

p = configargparse.ArgParser(description='Green Energy Hub events stream ingestor', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)
p.add('--storage-account-name', type=str, required=True)
p.add('--storage-account-key', type=str, required=True)
p.add('--event-hub-connection-key', type=str, required=True)
p.add('--delta-lake-container-name', type=str, required=True)
p.add('--events-data-blob-name', type=str, required=True)
p.add('--master-data-blob-name', type=str, required=True)

args, unknown_args = p.parse_known_args()

spark_conf = SparkConf(loadDefaults=True) \
    .set('fs.azure.account.key.{0}.dfs.core.windows.net'.format(args.storage_account_name), args.storage_account_key) \
    .set("spark.sql.session.timeZone", "UTC") \
    .set("spark.databricks.io.cache.enabled", "True") \

spark = SparkSession \
    .builder \
    .config(conf=spark_conf)\
    .getOrCreate()

events_delta_path = "abfss://" + args.delta_lake_container_name + "@" + args.storage_account_name + ".dfs.core.windows.net/" + args.events_data_blob_name

input_configuration = {}
input_configuration["eventhubs.connectionString"] = spark.sparkContext._gateway.jvm.org.apache.spark.eventhubs.EventHubsUtils.encrypt(args.event_hub_connection_key)

streamingDF = (spark.readStream.format("eventhubs").options(**input_configuration).load())
