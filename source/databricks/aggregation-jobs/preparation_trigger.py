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

from geh_stream.shared.data_loader import initialize_spark
from geh_stream.snapshot import create_snapshot

if __name__ == '__main__':
    p = configargparse.ArgParser(description='Green Energy Hub Tempory aggregation triggger', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)

    # Business settings
    p.add('--job-id', type=str, required=True)
    p.add('--grid-area', type=str, required=False,
          help='Run aggregation for specific grid areas format is { "areas": ["123","234"]}. If none is specifed. All grid areas are calculated')
    p.add('--beginning-date-time', type=str, required=True,
          help='The timezone aware date-time representing the beginning of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
    p.add('--end-date-time', type=str, required=True,
          help='The timezone aware date-time representing the end of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
    p.add('--snapshot-id', type=str, required=True)

    # Infrastructure settings
    p.add('--snapshot-notify-url', type=str, required=True, help="The target url to post result json")
    p.add('--snapshots-base-path', type=str, required=True)
    p.add('--time-series-points-delta-table-name', type=str, required=True, default="time-series-points", help='The time series points Delta table name')
    p.add('--shared-storage-account-name', type=str, required=True, help='Shared Azure Storage account name holding time series data')
    p.add('--shared-storage-account-key', type=str, required=True, help='Shared Azure Storage key for storage')
    p.add('--shared-storage-time-series-base-path', type=str, required=True, default='data', help='Shared Azure Storage time-series base path including container name')
    p.add('--shared-storage-aggregations-base-path', type=str, required=True, default='data', help='Shared Azure Storage aggregations base path including container name')
    p.add('--shared-database-url', type=str, required=True)
    p.add('--shared-database-aggregations', type=str, required=True)
    p.add('--shared-database-username', type=str, required=True)
    p.add('--shared-database-password', type=str, required=True)

    args, unknown_args = p.parse_known_args()

    areas = []
    if args.grid_area:
        areasParsed = json.loads(args.grid_area)
        areas = areasParsed["areas"]

    if unknown_args:
        print(f"Unknown args: {args}")

    spark = initialize_spark(args)

    create_snapshot(spark, areas, args)
