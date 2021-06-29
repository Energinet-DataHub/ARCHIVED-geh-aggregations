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
from datetime import datetime
from geh_stream.aggregation_utils.aggregators import \
    initialize_spark, \
    load_metering_points, \
    load_timeseries_dataframe, \
    load_grid_sys_cor_master_data_dataframe, \
    aggregate_net_exchange_per_ga, \
    aggregate_net_exchange_per_neighbour_ga, \
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
    combine_added_system_correction_with_master_data, \
    combine_added_grid_loss_with_master_data, \
    aggregate_quality

from geh_stream.aggregation_utils.services import PostProcessor

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
      help='The timezone aware date-time representing the beginning of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
p.add('--end-date-time', type=str, required=True,
      help='The timezone aware date-time representing the end of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
p.add('--telemetry-instrumentation-key', type=str, required=True,
      help='Instrumentation key used for telemetry')
p.add('--grid-area', type=str, required=False,
      help='Run aggregation for specific grid areas format is { "areas": ["123","234"]}. If none is specifed. All grid areas are calculated')
p.add('--process-type', type=str, required=True,
      help='D03 (Aggregation) or D04 (Balance fixing) '),
p.add('--result-url', type=str, required=True, help="The target url to post result json"),
p.add('--result-id', type=str, required=True, help="Postback id that will be added to header"),
p.add('--grid-loss-sys-cor-path', type=str, required=False, default="delta/grid-loss-sys-cor/")
p.add('--persist-source-dataframe', type=bool, required=False, default=False)
p.add('--persist-source-dataframe-location', type=str, required=False, default="delta/basis-data/")
p.add('--snapshot-url', type=str, required=True, help="The target url to post result json")
p.add('--cosmos-account-endpoint', type=str, required=True, help="")
p.add('--cosmos-account-key', type=str, required=True, help="")
p.add('--cosmos-database', type=str, required=True, help="")
args, unknown_args = p.parse_known_args()

areas = []

if args.grid_area:
    areasParsed = json.loads(args.grid_area)
    areas = areasParsed["areas"]
if unknown_args:
    print("Unknown args: {0}".format(args))

spark = initialize_spark(args)

filtered = load_timeseries_dataframe(args, areas, spark)

# Aggregate quality for aggregated timeseries grouped by grid area, market evaluation point type and time window
df = aggregate_quality(filtered)

# create a keyvalue dictionary for use in postprocessing each result are stored as a keyval with value being dataframe
results = {}

# STEP 1
results['net_exchange_per_neighbour_df'] = aggregate_net_exchange_per_neighbour_ga(df)

# STEP 2
results['net_exchange_per_ga_df'] = aggregate_net_exchange_per_ga(df)

# STEP 3
results['hourly_consumption_df'] = aggregate_hourly_consumption(df)

# STEP 4
flex_consumption_df = aggregate_flex_consumption(df) # This intermediate calculation is not dispatched to any market roles, hence not included in result set

# STEP 5
hourly_production_df = aggregate_hourly_production(df) # This intermediate calculation is not dispatched to any market roles, hence not included in result set

# STEP 6
results['grid_loss'] = calculate_grid_loss(results['net_exchange_per_ga_df'],
                                              results['hourly_consumption_df'],
                                              flex_consumption_df,
                                              hourly_production_df)

# STEP 8
added_system_correction_df = calculate_added_system_correction(results['grid_loss'])

# STEP 9
added_grid_loss_df = calculate_added_grid_loss(results['grid_loss'])

# Get additional data for grid loss and system correction
grid_loss_sys_cor_master_data_df = load_grid_sys_cor_master_data_dataframe(args, spark)

# Join additional data with added system correction
results['combined_system_correction'] = combine_added_system_correction_with_master_data(added_system_correction_df, grid_loss_sys_cor_master_data_df)

# Join additional data with added grid loss
results['combined_grid_loss'] = combine_added_grid_loss_with_master_data(added_system_correction_df, grid_loss_sys_cor_master_data_df)

# STEP 10
results['flex_consumption_with_grid_loss'] = adjust_flex_consumption(flex_consumption_df,
                                                                     added_grid_loss_df,
                                                                     grid_loss_sys_cor_master_data_df)

# STEP 11
results['hourly_production_with_system_correction_and_grid_loss'] = adjust_production(hourly_production_df,
                                                                                      added_system_correction_df,
                                                                                      grid_loss_sys_cor_master_data_df)

# STEP 12
results['hourly_production_ga_es'] = aggregate_per_ga_and_es(results['hourly_production_with_system_correction_and_grid_loss'])

# STEP 13
results['hourly_settled_consumption_ga_es'] = aggregate_per_ga_and_es(results['hourly_consumption_df'])

# STEP 14
results['flex_settled_consumption_ga_es'] = aggregate_per_ga_and_es(results['flex_consumption_with_grid_loss'])

# STEP 15
results['hourly_production_ga_brp'] = aggregate_per_ga_and_brp(results['hourly_production_with_system_correction_and_grid_loss'])

# STEP 16
results['hourly_settled_consumption_ga_brp'] = aggregate_per_ga_and_brp(results['hourly_consumption_df'])

# STEP 17
results['flex_settled_consumption_ga_brp'] = aggregate_per_ga_and_brp(results['flex_consumption_with_grid_loss'])

# STEP 18
results['hourly_production_ga'] = aggregate_per_ga(results['hourly_production_with_system_correction_and_grid_loss'])

# STEP 19
results['hourly_settled_consumption_ga'] = aggregate_per_ga(results['hourly_consumption_df'])

# STEP 20
results['flex_settled_consumption_ga'] = aggregate_per_ga(results['flex_consumption_with_grid_loss'])

# STEP 21
results['total_consumption'] = calculate_total_consumption(results['net_exchange_per_ga_df'], results['hourly_production_ga'])

# STEP 22
residual_ga = calculate_grid_loss(results['net_exchange_per_ga_df'],
                                             results['hourly_settled_consumption_ga'],
                                             results['flex_settled_consumption_ga'],
                                             results['hourly_production_ga'])

post_processor = PostProcessor(args)
now_path_string = datetime.now().strftime('%Y-%m-%d_%H-%M-%S')
post_processor.do_post_processing(args, results, now_path_string)
post_processor.store_basis_data(args, filtered, now_path_string)
 