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
from geh_stream.wholesale_utils.wholesale_initializer import get_hourly_charges
from geh_stream.wholesale_utils.calculators import calculate_tariff_price
from geh_stream.shared.services import PostProcessor
from geh_stream.codelists import BasisDataKeyName
from geh_stream.wholesale_utils.calculators import calculate_tariff_price, calculate_daily_subscription_price

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

# Dictionary containing raw data frames
snapshot_data = {}

# Load raw data frames based on date and grid area filters
time_series = load_time_series(args, spark, grid_areas)
snapshot_data[BasisDataKeyName.time_series_df] = time_series

metering_points = load_metering_points(args, spark, grid_areas)
snapshot_data[BasisDataKeyName.metering_points_df] = metering_points

charges = load_charges(args, spark)
snapshot_data[BasisDataKeyName.charges_df] = charges

charge_links = load_charge_links(args, spark)
snapshot_data[BasisDataKeyName.charge_links_df] = charge_links

charge_prices = load_charge_prices(args, spark)
snapshot_data[BasisDataKeyName.charge_prices_df] = charge_prices

market_roles = load_market_roles(args, spark)
snapshot_data[BasisDataKeyName.market_roles_df] = market_roles

gl_sc = load_grid_loss_sys_corr(args, spark, grid_areas)
snapshot_data[BasisDataKeyName.gl_sc_df] = gl_sc

es_brp_relations = load_es_brp_relations(args, spark, grid_areas)
snapshot_data[BasisDataKeyName.es_brp_relations_df] = es_brp_relations

# Store basis data
post_processor = PostProcessor(args)
post_processor.store_basis_data(args, snapshot_data)


# Initialize wholesale specific data frames
hourly_charges = get_hourly_charges(charges, charge_links, charge_prices)

results = {}

results['hourly_tariff'] = calculate_tariff_price(hourly_charges)

results['subscription_prices'] = calculate_daily_subscription_price(spark, charges, charge_links, charge_prices, metering_points, market_roles)
