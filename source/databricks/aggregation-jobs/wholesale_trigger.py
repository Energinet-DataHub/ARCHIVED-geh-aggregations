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

from geh_stream.aggregation_utils.trigger_base_arguments import trigger_base_arguments
import json
from geh_stream.shared.spark_initializer import initialize_spark
from geh_stream.shared.data_loader import load_metering_points, load_market_roles, load_charges, load_charge_links, load_charge_prices, load_es_brp_relations, load_grid_loss_sys_corr, load_time_series
from geh_stream.wholesale_utils.wholesale_initializer import get_charges
from geh_stream.wholesale_utils.calculators import calculate_tariff_price_per_ga_co_es
from geh_stream.codelists.resolution_duration import ResolutionDuration

p = trigger_base_arguments()
p.add('--cosmos-container-charges', type=str, required=True, help="Cosmos container for charges input data")
p.add('--cosmos-container-charge-links', type=str, required=True, help="Cosmos container for charge links input data")
p.add('--cosmos-container-charge-prices', type=str, required=True, help="Cosmos container for charge prices input data")

args, unknown_args = p.parse_known_args()

grid_areas = []

if args.grid_area:
    areasParsed = json.loads(args.grid_area)
    grid_areas = areasParsed["areas"]
if unknown_args:
    print("Unknown args: {0}".format(args))

spark = initialize_spark(args.data_storage_account_name, args.data_storage_account_key)

# Load raw data frames based on date and grid area filters
time_series = load_time_series(args, spark, grid_areas)

metering_points = load_metering_points(args, spark, grid_areas)

charges = load_charges(args, spark)

charge_links = load_charge_links(args, spark)

charge_prices = load_charge_prices(args, spark)

market_roles = load_market_roles(args, spark)

gl_sc = load_grid_loss_sys_corr(args, spark, grid_areas)

es_brp_relations = load_es_brp_relations(args, spark, grid_areas)

# Initialize wholesale specific data frames
daily_charges = get_charges(time_series, charges, charge_links, charge_prices, metering_points, market_roles, ResolutionDuration.day)

results = {}

results['hourly_tariff'] = calculate_tariff_price_per_ga_co_es(daily_charges)
