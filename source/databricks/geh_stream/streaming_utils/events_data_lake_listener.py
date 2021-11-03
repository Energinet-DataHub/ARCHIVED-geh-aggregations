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
from pyspark.sql import SparkSession
from geh_stream.event_dispatch.meteringpoint_dispatcher import dispatcher
from geh_stream.shared.data_exporter import export_to_csv
from geh_stream.bus import message_registry


def handle_event(type: str, body: str):
    event_class = message_registry.get(type)

    if event_class is not None:
        # deserialize from json with dataclasses_json
        try:
            event = event_class.from_json(body)
            dispatcher(event)
        except Exception as e:
            print("An exception occurred when trying to dispatch" + str(e))


def incomming_event_handler(df, epoch_id):
    if len(df.head(1)) > 0:
        df.rdd.map(lambda x: handle_event(x.type, x.body)).count()  # count to trigger lazy loading


def events_delta_lake_listener(delta_lake_container_name: str, storage_account_name: str, events_delta_path, master_data_path: str):
    inputDf = SparkSession.builder.getOrCreate().readStream.format("delta").load(events_delta_path)
    checkpoint_path = f"abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/event_delta_listener_streaming_checkpoint"

    dispatcher.set_master_data_root_path(master_data_path)

    stream = inputDf.writeStream.option("checkpointLocation", checkpoint_path).foreachBatch(lambda df, epochId: incomming_event_handler(df, epochId)).start()

    stream.awaitTermination()
