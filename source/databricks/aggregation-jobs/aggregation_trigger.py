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

# Uncomment the lines below to include modules distributed by wheel
import sys
sys.path.append(r'/workspaces/geh-aggregations/source/databricks')

import json
import configargparse
from azure.storage.blob import BlobServiceClient
from azure.core.exceptions import ResourceNotFoundError
from geh_stream.aggregation_utils.aggregators import \
    initialize_dataframe, \
    aggregate_net_exchange_per_neighbour_ga, \
    aggregate_net_exchange_per_ga, \
    aggregate_hourly_consumption, \
    aggregate_flex_consumption, \
    aggregate_hourly_production, \
    aggregate_per_ga_and_es, \
    aggregate_per_ga_and_brp, \
    aggregate_per_ga, \
    calculate_grid_loss, \
    calculate_added_system_correction, \
    calculate_added_grid_loss, \
    calculate_total_consumption, \
    adjust_flex_consumption, \
    adjust_production, \
    GridLossSysCorRepo
from geh_stream.aggregation_utils.services import CoordinatorService
from geh_stream.DTOs.AggregationResults import AggregationResults


def upload_blob(data, blob_name):
    blob_service_client = BlobServiceClient.from_connection_string("DefaultEndpointsProtocol=https;AccountName=timeseriesdatajouless;AccountKey=KIGkX7XIGilmUwADO5qT7k4AzFk8S0nl5QmfBD3+ktHxqhoVwfQUPeV7DpMxvhmTNVe/bcdJwA6PIyUHuvqKZw==;EndpointSuffix=core.windows.net")
    blob_client = blob_service_client.get_blob_client(container="messagedata", blob=blob_name)
    try:
        blob_client.get_blob_properties()
        blob_client.delete_blob()
    except ResourceNotFoundError:
        pass
    blob_client.upload_blob(data)


p = configargparse.ArgParser(description='Green Energy Hub Tempory aggregation triggger', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)
p.add('--input-storage-account-name', type=str, required=True,
      help='Azure Storage account name holding time series data')
p.add('--input-storage-account-key', type=str, required=True,
      help='Azure Storage key for input storage', env_var='GEH_INPUT_STORAGE_KEY')
p.add('--input-storage-container-name', type=str, required=False, default='data',
      help='Azure Storage container name for input storage')
p.add('--input-path', type=str, required=False, default="delta/meter-data/",
      help='Path to time series data storage location (deltalake) relative to root container')
p.add('--beginning-date-time', type=str, required=True,
      help='The timezone aware date-time representing the beginning of the time period of aggregation (ex: 2020-01-03T00:00:00+0100 %Y-%m-%dT%H:%M:%S%z)')
p.add('--end-date-time', type=str, required=True,
      help='The timezone aware date-time representing the end of the time period of aggregation (ex: 2020-01-03T00:00:00-0100 %Y-%m-%dT%H:%M:%S%z)')
p.add('--telemetry-instrumentation-key', type=str, required=True,
      help='Instrumentation key used for telemetry')
p.add('--grid-area', type=str, required=False,
      help='Run aggregation for specific grid areas format is { "areas": ["123","234"]}. If none is specifed. All grid areas are calculated')
p.add('--process-type', type=str, required=True,
      help='D03 (Aggregation) or D04 (Balance fixing) '),
p.add('--result-url', type=str, required=True, help="The target url to post result json")
p.add('--result-id', type=str, required=True, help="Postback id that will be added to header")


args, unknown_args = p.parse_known_args()

areas = []

if args.grid_area:
    areasParsed = json.loads(args.grid_area)
    areas = areasParsed["areas"]
if unknown_args:
    print("Unknown args:")
    _ = [print(arg) for arg in unknown_args]

 df = initialize_dataframe(args, areas)

# STEP 1
net_exchange_per_neighbour_df = aggregate_net_exchange_per_neighbour_ga(df)
upload_blob(net_exchange_per_neighbour_df.toJSON().collect(), "test-output/net_exchange_per_ga_df.json")

# STEP 2
net_exchange_per_ga_df = aggregate_net_exchange_per_ga(df)

# STEP 3
hourly_consumption_df = aggregate_hourly_consumption(df)
# STEP 4
flex_consumption_df = aggregate_flex_consumption(df)
# STEP 5
hourly_production_df = aggregate_hourly_production(df)

# STEP 6
grid_loss_df = calculate_grid_loss(net_exchange_per_ga_df,
                                   hourly_consumption_df,
                                   flex_consumption_df,
                                   hourly_production_df)

# STEP 8
added_system_correction_df = calculate_added_system_correction(grid_loss_df)
# STEP 9
added_grid_loss_df = calculate_added_grid_loss(grid_loss_df)

gridLossSysCorRepo = GridLossSysCorRepo()
grid_loss_sys_cor_df = gridLossSysCorRepo.get_df()

# STEP 10
flex_consumption_with_grid_loss = adjust_flex_consumption(flex_consumption_df, added_grid_loss_df, grid_loss_sys_cor_df)

# STEP 11
hourly_production_with_system_correction_and_grid_loss = adjust_production(hourly_production_df, added_system_correction_df, grid_loss_sys_cor_df)

# STEP 12
hourly_production_ga_es = aggregate_per_ga_and_es(hourly_production_with_system_correction_and_grid_loss)

# STEP 13
hourly_settled_consumption_ga_es = aggregate_per_ga_and_es(hourly_consumption_df)

# STEP 14
flex_settled_consumption_ga_es = aggregate_per_ga_and_es(flex_consumption_with_grid_loss)

# STEP 15
hourly_production_ga_brp = aggregate_per_ga_and_brp(hourly_production_with_system_correction_and_grid_loss)

# STEP 16
hourly_settled_consumption_ga_brp = aggregate_per_ga_and_brp(hourly_consumption_df)

# STEP 17
flex_settled_consumption_ga_brp = aggregate_per_ga_and_brp(flex_consumption_with_grid_loss)

# STEP 18
hourly_production_ga = aggregate_per_ga(hourly_production_with_system_correction_and_grid_loss)

# STEP 19
hourly_settled_consumption_ga = aggregate_per_ga(hourly_consumption_df)

# STEP 20
flex_settled_consumption_ga = aggregate_per_ga(flex_consumption_with_grid_loss)

# STEP 21
total_consumption = calculate_total_consumption(net_exchange_per_ga_df, hourly_production_ga)

# STEP 22
residual_ga = calculate_grid_loss(net_exchange_per_ga_df,
                                  hourly_settled_consumption_ga,
                                  flex_settled_consumption_ga,
                                  hourly_production_ga)


#output_delta_lake_path = "abfss://{0}@{1}.dfs.core.windows.net/{2}".format(args.input_storage_container_name, args.input_storage_account_name, "test-output/net_exchange_per_ga_df")

net_exchange_per_ga_df.write \
                      .format("delta") \
                      .save(output_delta_lake_path)

# aggregation_results = AggregationResults(net_exchange_per_neighbour_df.toJSON().collect(),
#                                         hourly_consumption_df.toJSON().collect(),
#                                         hourly_production_df.toJSON().collect(),
#                                         flex_consumption_df.toJSON().collect(),
#                                         flex_consumption_with_grid_loss.toJSON().collect(),
#                                         hourly_production_with_system_correction_and_grid_loss.toJSON().collect())


#coordinator_service = CoordinatorService(args)
#coordinator_service.send_result_to_coordinator(aggregation_results.toJSON())
