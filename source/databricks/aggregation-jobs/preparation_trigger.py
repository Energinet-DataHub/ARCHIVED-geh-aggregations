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
from geh_stream.shared.services import InputOutputProcessor
from geh_stream.aggregation_utils.aggregators import \
    load_metering_points, \
    load_time_series, \
    load_market_roles, \
    load_es_brp_relations, \
    load_charges, \
    load_charge_links, \
    load_charge_prices, \
    initialize_spark

from geh_stream.codelists import BasisDataKeyName

p = trigger_base_arguments()

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
snapshot_data[BasisDataKeyName.time_series] = load_time_series(args, areas, spark)

# Fetch metering point df
snapshot_data[BasisDataKeyName.metering_points] = load_metering_points(args, spark)

# Fetch market roles df
snapshot_data[BasisDataKeyName.market_roles] = load_market_roles(args, spark)

# Fetch energy supplier, balance responsible relations df
snapshot_data[BasisDataKeyName.es_brp_relations] = load_es_brp_relations(args, spark)

# Fetch charges for wholesale
snapshot_data[BasisDataKeyName.charges] = load_charges(args, spark)

# Fetch charge links for wholesale
snapshot_data[BasisDataKeyName.charge_links] = load_charge_links(args, spark)

# Fetch charge prices for wholesale
snapshot_data[BasisDataKeyName.charge_prices] = load_charge_prices(args, spark)


# Store basis data
post_processor = InputOutputProcessor(args)
post_processor.store_basis_data(args, snapshot_data)
