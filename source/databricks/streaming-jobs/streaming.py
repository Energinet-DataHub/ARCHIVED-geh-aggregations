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
import sys
import configargparse
import json
import datetime
sys.path.append(r'/workspaces/geh-aggregations/source/databricks')

from pyspark.sql import DataFrame
from pyspark.sql.functions import col, from_json, explode

from pyspark import SparkConf
from pyspark.sql.session import SparkSession
from geh_stream.shared.spark_initializer import initialize_spark
from eventhub_ingestor import events_ingenstion_stream
from events_data_lake_listener import events_delta_lake_listener

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

try:
    spark = initialize_spark(args.storage_account_name, args.storage_account_key)
except IndexError:
    print("An expected exception occurred")

events_delta_path = "abfss://" + args.delta_lake_container_name + "@" + args.storage_account_name + ".dfs.core.windows.net/" + args.events_data_blob_name

# start the eventhub ingestor
events_ingenstion_stream(spark, args.event_hub_connection_key, args.delta_lake_container_name, args.storage_account_name, events_delta_path)

# start the delta lake event listener
events_delta_lake_listener(spark, args.delta_lake_container_name, args.storage_account_name, events_delta_path)
