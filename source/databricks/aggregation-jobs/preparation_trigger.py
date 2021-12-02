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
import configargparse

from geh_stream.shared.services import InputOutputProcessor
from geh_stream.shared.data_loader import \
    load_metering_points, \
    load_time_series, \
    load_market_roles, \
    load_es_brp_relations, \
    load_charges, \
    load_charge_links, \
    load_charge_prices, \
    load_grid_loss_sys_corr, \
    initialize_spark

from geh_stream.codelists import BasisDataKeyName
from geh_stream.aggregation_utils.trigger_base_arguments import trigger_base_arguments

p = trigger_base_arguments()
p.add('--grid-area', type=str, required=False, help='Run aggregation for specific grid areas format is { "areas": ["123","234"]}. If none is specifed. All grid areas are calculated')
p.add('--snapshot-url', type=str, required=True, help="The target url to post result json")
p.add('--time-series-path', type=str, required=True, default="timeseries", help='Path to time series data storage location (deltalake) relative to root container')
p.add('--metering-points-path', type=str, required=True, default="master-data/metering-points", help='Path to metering points data storage location (deltalake) relative to root container')
p.add('--market-roles-path', type=str, required=True, default="master-data/market-roles", help='Path to market roles data storage location (deltalake) relative to root container')
p.add('--charges-path', type=str, required=True, default="master-data/charges", help='Path to charges data storage location (deltalake) relative to root container')
p.add('--charge-links-path', type=str, required=True, default="master-data/charge-links", help='Path to charge links data storage location (deltalake) relative to root container')
p.add('--charge-prices-path', type=str, required=True, default="master-data/charge-prices", help='Path to charge prices data storage location (deltalake) relative to root container')
p.add('--es-brp-relations-path', type=str, required=True, default="master-data/es-brp-relations", help='Path to es brp relations data storage location (deltalake) relative to root container')
p.add('--grid-loss-system-correction-path', type=str, required=True, default="master-data/grid-loss-system-correction", help='Path to grid loss system correction data storage location (deltalake) relative to root container')
p.add('--beginning-date-time', type=str, required=True, help='The timezone aware date-time representing the beginning of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
p.add('--end-date-time', type=str, required=True, help='The timezone aware date-time representing the end of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')

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

# Fetch time series dataframe
snapshot_data[BasisDataKeyName.time_series] = load_time_series(args, spark, areas)

# Fetch metering point df
snapshot_data[BasisDataKeyName.metering_points] = load_metering_points(args, spark, areas)

# Fetch market roles df
snapshot_data[BasisDataKeyName.market_roles] = load_market_roles(args, spark)

# Fetch energy supplier, balance responsible relations df
snapshot_data[BasisDataKeyName.es_brp_relations] = load_es_brp_relations(args, spark, areas)

# Fetch charges for wholesale
snapshot_data[BasisDataKeyName.charges] = load_charges(args, spark)

# Fetch charge links for wholesale
snapshot_data[BasisDataKeyName.charge_links] = load_charge_links(args, spark)

# Fetch charge prices for wholesale
snapshot_data[BasisDataKeyName.charge_prices] = load_charge_prices(args, spark)

# Fetch system correction metering points
snapshot_data[BasisDataKeyName.grid_loss_sys_corr] = load_grid_loss_sys_corr(args, spark, areas)


# Store basis data
io_processor = InputOutputProcessor(args)
io_processor.store_basis_data(args.snapshot_url, snapshot_data)
