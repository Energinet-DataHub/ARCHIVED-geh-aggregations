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
import traceback
from pyspark.sql import *
from pyspark.sql.functions import from_json
from geh_stream.event_dispatch.meteringpoint_dispatcher import dispatcher
from geh_stream.shared.data_exporter import export_to_csv
from geh_stream.bus import message_registry
from geh_stream.shared.data_loader import initialize_spark
from geh_stream.schemas import metering_point_schema

def map_to_masterdata(partition, master_data_path, args):
    # Since this code runs on a seperate instance (executer node), we need to initialize our spark context again
    spark = initialize_spark(args)

    for row in partition:
        event_class = message_registry.get(row["type"])
        dispatcher.set_master_data_root_path(master_data_path)

        if event_class is not None:
            # deserialize from json with dataclasses_json
            body = row["body"]
            try:
                event = event_class.from_json(body)
                dispatcher(event, master_data_path)
            except Exception as e:
                ex_type, ex_value, ex_traceback = sys.exc_info()
                # Extract unformatter stack traces as tuples
                trace_back = traceback.extract_tb(ex_traceback)

                # Format stacktrace
                stack_trace = list()

                for trace in trace_back:
                    stack_trace.append("File : %s , Line : %d, Func.Name : %s, Message : %s" % (trace[0], trace[1], trace[2], trace[3]))

                print("Exception type : %s " % ex_type.__name__)
                print("Exception message : %s" %ex_value)
                print("Stack trace : %s" %stack_trace)
                print(f"body : {body}")


def incomming_event_handler(batchDF, epoch_id, master_data_path, args):
    if len(batchDF.head(1)) > 0:
        df = batchDF.cache()
        obj = df.select(from_json(df.body, metering_point_schema).alias("json"))
        print(obj.show())
        # df.foreachPartition(lambda partition: map_to_masterdata(partition, master_data_path, args))
        # filter df by type
        # foreachpartition
        # repartition by X


def events_delta_lake_listener(delta_lake_container_name: str, storage_account_name: str, events_delta_path, master_data_path: str, args):
    inputDf = SparkSession.builder.getOrCreate().readStream.format("delta").load(events_delta_path)
    checkpoint_path = f"abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/event_delta_listener_streaming_checkpoint"

    stream = inputDf.writeStream.option("checkpointLocation", checkpoint_path).foreachBatch(lambda df, epochId: incomming_event_handler(df, epochId, master_data_path, args)).start()

    stream.awaitTermination()
