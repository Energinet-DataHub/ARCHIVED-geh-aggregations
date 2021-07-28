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

import configargparse
import json

p = configargparse.ArgParser(description='Green Energy Hub Tempory aggregation triggger', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)
p.add('--input-storage-account-name', type=str, required=True,
      help='Azure Storage account name holding time series data')
p.add('--input-storage-account-key', type=str, required=True,
      help='Azure Storage key for input storage', env_var='GEH_INPUT_STORAGE_KEY')
p.add('--input-storage-container-name', type=str, required=False, default='data',
      help='Azure Storage container name for input storage')
p.add('--input-path', type=str, required=False, default="delta/time_series_test_data/",
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