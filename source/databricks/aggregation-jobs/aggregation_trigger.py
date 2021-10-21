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
import json
from geh_stream.shared.data_classes import Metadata
from geh_stream.shared.data_exporter import export_to_csv
from geh_stream.aggregation_utils.trigger_base_arguments import trigger_base_arguments
from geh_stream.shared.data_loader import initialize_spark
from geh_stream.aggregation_utils.aggregators import \
    get_time_series_dataframe, \
    aggregate_net_exchange_per_ga, \
    aggregate_net_exchange_per_neighbour_ga, \
    aggregate_hourly_consumption, \
    aggregate_flex_consumption, \
    aggregate_hourly_production, \
    aggregate_hourly_production_ga_es, \
    aggregate_hourly_settled_consumption_ga_es, \
    aggregate_flex_settled_consumption_ga_es, \
    aggregate_hourly_production_ga_brp, \
    aggregate_hourly_settled_consumption_ga_brp, \
    aggregate_flex_settled_consumption_ga_brp, \
    aggregate_hourly_production_ga, \
    aggregate_hourly_settled_consumption_ga, \
    aggregate_flex_settled_consumption_ga, \
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
p.add('--meta-data-dictionary', type=dict, required=True, help="Meta data dictionary")
args, unknown_args = p.parse_known_args()

spark = initialize_spark(args)

io_processor = InputOutputProcessor(args)

meta_data_dict = json.loads(args.meta_data_dictionary)


# Add raw dataframes to basis data dictionary and return joined dataframe
filtered = get_time_series_dataframe(io_processor.load_basis_data(spark, BasisDataKeyName.time_series),
                                     io_processor.load_basis_data(spark, BasisDataKeyName.metering_points),
                                     io_processor.load_basis_data(spark, BasisDataKeyName.market_roles),
                                     io_processor.load_basis_data(spark, BasisDataKeyName.es_brp_relations))

results = {}
# Aggregate quality for aggregated timeseries grouped by grid area, market evaluation point type and time window
results[ResultKeyName.aggregation_base_dataframe] = aggregate_quality(filtered)

# Get additional data for grid loss and system correction
results[ResultKeyName.grid_loss_sys_cor_master_data] = io_processor.load_basis_data(spark, BasisDataKeyName.grid_loss_sys_corr)

# Create a keyvalue dictionary for use in postprocessing. Each result are stored as a keyval with value being dataframe

functions = {
    10: aggregate_net_exchange_per_neighbour_ga,
    20: aggregate_net_exchange_per_ga,
    30: aggregate_hourly_consumption,
    40: aggregate_flex_consumption,
    50: aggregate_hourly_production,
    60: calculate_grid_loss,
    70: calculate_added_system_correction,
    80: calculate_added_grid_loss,
    90: combine_added_system_correction_with_master_data,
    100: combine_added_grid_loss_with_master_data,
    110: adjust_flex_consumption,
    120: adjust_production,
    130: aggregate_hourly_production_ga_es,
    140: aggregate_hourly_settled_consumption_ga_es,
    150: aggregate_flex_settled_consumption_ga_es,
    160: aggregate_hourly_production_ga_brp,
    170: aggregate_hourly_settled_consumption_ga_brp,
    180: aggregate_flex_settled_consumption_ga_brp,
    190: aggregate_hourly_production_ga,
    200: aggregate_hourly_settled_consumption_ga,
    210: aggregate_flex_settled_consumption_ga,
    220: calculate_total_consumption,
    230: calculate_grid_loss
}

for key, value in meta_data_dict.items():
    key = int(key)
    results[key] = functions[key](results, Metadata(**value))



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
io_processor.do_post_processing(args.process_type, args.job_id, args.result_url, results)
