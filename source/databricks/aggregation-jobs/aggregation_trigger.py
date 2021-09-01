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

import configargparse
from geh_stream.shared.data_exporter import export_to_csv
from geh_stream.aggregation_utils.trigger_base_arguments import trigger_base_arguments
from geh_stream.aggregation_utils.aggregators import \
    initialize_spark, \
    get_time_series_dataframe, \
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

from geh_stream.shared.services import InputOutputProcessor
from geh_stream.codelists import BasisDataKeyName, ResultKeyName

p = trigger_base_arguments()

p.add('--resolution', type=str, required=True, help="Time window resolution eg. 60 minutes, 15 minutes etc.")
args, unknown_args = p.parse_known_args()

spark = initialize_spark(args)

io_processor = InputOutputProcessor(args)

# Add raw dataframes to basis data dictionary and return joined dataframe
filtered = get_time_series_dataframe(io_processor.load_basis_data(spark, BasisDataKeyName.time_series),
                                     io_processor.load_basis_data(spark, BasisDataKeyName.metering_points),
                                     io_processor.load_basis_data(spark, BasisDataKeyName.market_roles),
                                     io_processor.load_basis_data(spark, BasisDataKeyName.es_brp_relations))

# Aggregate quality for aggregated timeseries grouped by grid area, market evaluation point type and time window
df = aggregate_quality(filtered)

# Create a keyvalue dictionary for use in postprocessing. Each result are stored as a keyval with value being dataframe
results = {}

# STEP 1
results[ResultKeyName.net_exchange_per_neighbour] = aggregate_net_exchange_per_neighbour_ga(df)

# STEP 2
results[ResultKeyName.net_exchange_per_ga] = aggregate_net_exchange_per_ga(df)

# STEP 3
results[ResultKeyName.hourly_consumption] = aggregate_hourly_consumption(df)

# STEP 4
flex_consumption_df = aggregate_flex_consumption(df)        # This intermediate calculation is not dispatched to any market roles, hence not included in result set

# STEP 5
hourly_production_df = aggregate_hourly_production(df)      # This intermediate calculation is not dispatched to any market roles, hence not included in result set

# STEP 6
results[ResultKeyName.grid_loss] = calculate_grid_loss(results[ResultKeyName.net_exchange_per_ga],
                                                       results[ResultKeyName.hourly_consumption],
                                                       flex_consumption_df,
                                                       hourly_production_df)
# STEP 8
added_system_correction_df = calculate_added_system_correction(results[ResultKeyName.grid_loss])

# STEP 9
added_grid_loss_df = calculate_added_grid_loss(results[ResultKeyName.grid_loss])

# Get additional data for grid loss and system correction
grid_loss_sys_cor_master_data_df = io_processor.load_basis_data(spark, BasisDataKeyName.grid_loss_sys_corr)

# Join additional data with added system correction
results[ResultKeyName.combined_system_correction] = combine_added_system_correction_with_master_data(added_system_correction_df, grid_loss_sys_cor_master_data_df)
# Join additional data with added grid loss
results[ResultKeyName.combined_grid_loss] = combine_added_grid_loss_with_master_data(added_system_correction_df, grid_loss_sys_cor_master_data_df)

# STEP 10
results[ResultKeyName.flex_consumption_with_grid_loss] = adjust_flex_consumption(flex_consumption_df,
                                                                                 added_grid_loss_df,
                                                                                 grid_loss_sys_cor_master_data_df)
# STEP 11
results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss] = adjust_production(hourly_production_df,
                                                                                                  added_system_correction_df,
                                                                                                  grid_loss_sys_cor_master_data_df)

# STEP 12
results[ResultKeyName.hourly_production_ga_es] = aggregate_per_ga_and_es(results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss])

# STEP 13
results[ResultKeyName.hourly_settled_consumption_ga_es] = aggregate_per_ga_and_es(results[ResultKeyName.hourly_consumption])

# STEP 14
results[ResultKeyName.flex_settled_consumption_ga_es] = aggregate_per_ga_and_es(results[ResultKeyName.flex_consumption_with_grid_loss])

# STEP 15
results[ResultKeyName.hourly_production_ga_brp] = aggregate_per_ga_and_brp(results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss])

# STEP 16
results[ResultKeyName.hourly_settled_consumption_ga_brp] = aggregate_per_ga_and_brp(results[ResultKeyName.hourly_consumption])

# STEP 17
results[ResultKeyName.flex_settled_consumption_ga_brp] = aggregate_per_ga_and_brp(results[ResultKeyName.flex_consumption_with_grid_loss])

# STEP 18
results[ResultKeyName.hourly_production_ga] = aggregate_per_ga(results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss])

# STEP 19
results[ResultKeyName.hourly_settled_consumption_ga] = aggregate_per_ga(results[ResultKeyName.hourly_consumption])

# STEP 20
results[ResultKeyName.flex_settled_consumption_ga] = aggregate_per_ga(results[ResultKeyName.flex_consumption_with_grid_loss])

# STEP 21
results[ResultKeyName.total_consumption] = calculate_total_consumption(results[ResultKeyName.net_exchange_per_ga], results[ResultKeyName.hourly_production_ga])

# STEP 22
residual_ga = calculate_grid_loss(results[ResultKeyName.net_exchange_per_ga],
                                  results[ResultKeyName.hourly_settled_consumption_ga],
                                  results[ResultKeyName.flex_settled_consumption_ga],
                                  results[ResultKeyName.hourly_production_ga])


# Enable to dump results to local csv files
# export_to_csv(results)

# Store aggregation results
io_processor.do_post_processing(args.process_type, args.result_id, args.result_url, results)
