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
sys.path.append(r'/opt/conda/lib/python3.8/site-packages')

import json
from geh_stream.aggregation_utils.trigger_base_arguments import trigger_base_arguments
from geh_stream.shared.data_exporter import export_to_csv
from geh_stream.aggregation_utils.aggregators import \
    initialize_spark, \
    load_metering_points, \
    load_time_series, \
    get_time_series_dataframe, \
    get_translated_grid_loss_sys_corr, \
    load_market_roles, \
    load_es_brp_relations, \
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

from geh_stream.shared.services import PostProcessor
from geh_stream.codelists import BasisDataKeyName, ResultKeyName

p = trigger_base_arguments()
p.add('--resolution', type=str, required=True, help="Time window resolution eg. 60 minutes, 15 minutes etc.")

args, unknown_args = p.parse_known_args()

areas = []

if args.grid_area:
    areasParsed = json.loads(args.grid_area)
    areas = areasParsed["areas"]
if unknown_args:
    print(f"Unknown args: {args}")

spark = initialize_spark(args)

# Create a keyvalue dictionary for use in store basis data. Each snapshot data are stored as a keyval with value being dataframe
snapshot_data = {}
post_processor = PostProcessor(args)

# Fetch time series dataframe
snapshot_data[BasisDataKeyName.time_series_df] = load_time_series(args, areas, spark)

# Fetch metering point df
snapshot_data[BasisDataKeyName.metering_point_df] = load_metering_points(args, spark)

# Fetch market roles df
snapshot_data[BasisDataKeyName.market_roles_df] = load_market_roles(args, spark)

# Fetch energy supplier, balance responsible relations df
snapshot_data[BasisDataKeyName.es_brp_relations_df] = load_es_brp_relations(args, spark)

# Add raw dataframes to basis data dictionary and return joined dataframe
filtered = get_time_series_dataframe(snapshot_data[BasisDataKeyName.time_series_df],
                                     snapshot_data[BasisDataKeyName.metering_point_df],
                                     snapshot_data[BasisDataKeyName.market_roles_df],
                                     snapshot_data[BasisDataKeyName.es_brp_relations_df])

# Store basis data
post_processor.store_basis_data(args, snapshot_data)

# Aggregate quality for aggregated timeseries grouped by grid area, market evaluation point type and time window
df = aggregate_quality(filtered)

# Create a keyvalue dictionary for use in postprocessing. Each result are stored as a keyval with value being dataframe
results = {}

# STEP 1
results[ResultKeyName.net_exchange_per_neighbour_df] = aggregate_net_exchange_per_neighbour_ga(df)

# STEP 2
results[ResultKeyName.net_exchange_per_ga_df] = aggregate_net_exchange_per_ga(df)

# STEP 3
results[ResultKeyName.hourly_consumption_df] = aggregate_hourly_consumption(df)

# STEP 4
flex_consumption_df = aggregate_flex_consumption(df)        # This intermediate calculation is not dispatched to any market roles, hence not included in result set

# STEP 5
hourly_production_df = aggregate_hourly_production(df)      # This intermediate calculation is not dispatched to any market roles, hence not included in result set

# STEP 6
results[ResultKeyName.grid_loss_df] = calculate_grid_loss(results[ResultKeyName.net_exchange_per_ga_df],
                                                          results[ResultKeyName.hourly_consumption_df],
                                                          flex_consumption_df,
                                                          hourly_production_df)
# STEP 8
added_system_correction_df = calculate_added_system_correction(results[ResultKeyName.grid_loss_df])

# STEP 9
added_grid_loss_df = calculate_added_grid_loss(results[ResultKeyName.grid_loss_df])

# Get additional data for grid loss and system correction
grid_loss_sys_cor_master_data_df = get_translated_grid_loss_sys_corr(args, spark)

# Join additional data with added system correction
results[ResultKeyName.combined_system_correction_df] = combine_added_system_correction_with_master_data(added_system_correction_df, grid_loss_sys_cor_master_data_df)
# Join additional data with added grid loss
results[ResultKeyName.combined_grid_loss_df] = combine_added_grid_loss_with_master_data(added_system_correction_df, grid_loss_sys_cor_master_data_df)

# STEP 10
results[ResultKeyName.flex_consumption_with_grid_loss_df] = adjust_flex_consumption(flex_consumption_df,
                                                                                    added_grid_loss_df,
                                                                                    grid_loss_sys_cor_master_data_df)
# STEP 11
results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss_df] = adjust_production(hourly_production_df,
                                                                                                     added_system_correction_df,
                                                                                                     grid_loss_sys_cor_master_data_df)

# STEP 12
results[ResultKeyName.hourly_production_ga_es_df] = aggregate_per_ga_and_es(results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss_df])

# STEP 13
results[ResultKeyName.hourly_settled_consumption_ga_es_df] = aggregate_per_ga_and_es(results[ResultKeyName.hourly_consumption_df])

# STEP 14
results[ResultKeyName.flex_settled_consumption_ga_es_df] = aggregate_per_ga_and_es(results[ResultKeyName.flex_consumption_with_grid_loss_df])

# STEP 15
results[ResultKeyName.hourly_production_ga_brp_df] = aggregate_per_ga_and_brp(results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss_df])

# STEP 16
results[ResultKeyName.hourly_settled_consumption_ga_brp_df] = aggregate_per_ga_and_brp(results[ResultKeyName.hourly_consumption_df])

# STEP 17
results[ResultKeyName.flex_settled_consumption_ga_brp_df] = aggregate_per_ga_and_brp(results[ResultKeyName.flex_consumption_with_grid_loss_df])

# STEP 18
results[ResultKeyName.hourly_production_ga_df] = aggregate_per_ga(results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss_df])

# STEP 19
results[ResultKeyName.hourly_settled_consumption_ga_df] = aggregate_per_ga(results[ResultKeyName.hourly_consumption_df])

# STEP 20
results[ResultKeyName.flex_settled_consumption_ga_df] = aggregate_per_ga(results[ResultKeyName.flex_consumption_with_grid_loss_df])

# STEP 21
results[ResultKeyName.total_consumption_df] = calculate_total_consumption(results[ResultKeyName.net_exchange_per_ga_df], results[ResultKeyName.hourly_production_ga_df])

# STEP 22
residual_ga = calculate_grid_loss(results[ResultKeyName.net_exchange_per_ga_df],
                                  results[ResultKeyName.hourly_settled_consumption_ga_df],
                                  results[ResultKeyName.flex_settled_consumption_ga_df],
                                  results[ResultKeyName.hourly_production_ga_df])


# Enable to dump results to local csv files
# export_to_csv(results)

# Store aggregation results
post_processor.do_post_processing(args, results)
