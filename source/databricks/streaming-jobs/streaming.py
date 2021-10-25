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
sys.path.append(r'/workspaces/geh-aggregations/source/databricks')
sys.path.append(r'/opt/conda/lib/python3.8/site-packages')

import configargparse

from geh_stream.streaming_utils.eventhub_ingestor import events_ingenstion_stream
from geh_stream.streaming_utils.events_data_lake_listener import events_delta_lake_listener

from geh_stream.shared.data_loader import initialize_spark

p = configargparse.ArgParser(description='Green Energy Hub events stream ingestor', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)
p.add('--data-storage-account-name', type=str, required=True)
p.add('--data-storage-account-key', type=str, required=True)
p.add('--event-hub-connection-key', type=str, required=True)
p.add('--delta-lake-container-name', type=str, required=True)
p.add('--events-data-blob-name', type=str, required=True)
p.add('--master-data-blob-name', type=str, required=True)

args, unknown_args = p.parse_known_args()

spark = initialize_spark(args)

events_delta_path = f"abfss://{args.delta_lake_container_name}@{args.data_storage_account_name}.dfs.core.windows.net/{args.events_data_blob_name}"
master_data_path = f"abfss://{args.delta_lake_container_name}@{args.data_storage_account_name}.dfs.core.windows.net/{args.master_data_blob_name}"

# start the eventhub ingestor
events_ingenstion_stream(args.event_hub_connection_key, args.delta_lake_container_name, args.data_storage_account_name, events_delta_path)

# start the delta lake event listener
events_delta_lake_listener(args.delta_lake_container_name, args.data_storage_account_name, events_delta_path, master_data_path)
