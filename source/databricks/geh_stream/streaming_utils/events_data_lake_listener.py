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
import json
from pyspark.sql import SparkSession
from pyspark.sql import col
from pyspark.sql.types import StringType
from geh_stream.event_dispatch import dispatcher


def incomming_event_handler(df, epoch_id, spark: SparkSession):
    if len(df.head(1)) > 0:
        # df.show()
        
        # create event object by doing class reflection  #https://stackoverflow.com/questions/553784/can-you-use-a-string-to-instantiate-a-class
        constructor = globals()[df.select(col("SchemaType"))]
        eventObject = constructor(spark)
        # deserialize from json with dataclasses_json
        eventObject.from_json(df.select(col("body")))
        dispatcher(eventObject)
        # Id,
        # SchemaType
        # body


def events_delta_lake_listener(spark: SparkSession, delta_lake_container_name: str, storage_account_name: str, events_delta_path):
    inputDf = spark.readStream.format("delta").load(events_delta_path)
    checkpoint_path = "abfss://" + delta_lake_container_name + "@" + storage_account_name + ".dfs.core.windows.net/event_delta_listener_streaming_checkpoint"
    inputDf.writeStream.option("checkpointLocation", checkpoint_path).foreachBatch(lambda df, epochId: incomming_event_handler(df, epochId, spark)).start()
