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
from pyspark.sql.types import DecimalType, StructType, StructField, StringType, TimestampType, ArrayType, BinaryType, IntegerType

from pyspark import SparkConf
from pyspark.sql.session import SparkSession
from geh_stream.shared.spark_initializer import initialize_spark

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
except IndexError as error:
    print(f"An expected exception occurred, {error}")

events_delta_path = "abfss://" + args.delta_lake_container_name + "@" + args.storage_account_name + ".dfs.core.windows.net/" + args.events_data_blob_name

input_configuration = {}
input_configuration["eventhubs.connectionString"] = spark.sparkContext._gateway.jvm.org.apache.spark.eventhubs.EventHubsUtils.encrypt(args.event_hub_connection_key)

streamingDF = (spark.readStream.format("eventhubs").options(**input_configuration).load())

event_schema = StructType([StructField("id", StringType(), False), StructField("type", StringType(), False), StructField("payload", TimestampType(), False)])


def foreach_batch_function(df, epoch_id):
    if len(df.head(1)) > 0:
        # Extract metadata from the eventhub message and wrap into containing dataframe
        jsonDataFrame = df.select((df.properties["Id"]).alias("Id"), (df.properties["SchemaType"]).alias("type"), (df.body.cast(StringType()).alias("body")))

    # Append event
        jsonDataFrame.write \
            .partitionBy("type") \
            .format("delta") \
            .mode("append") \
            .save(events_delta_path)


streamingDF.writeStream.foreachBatch(foreach_batch_function).start().awaitTermination()
