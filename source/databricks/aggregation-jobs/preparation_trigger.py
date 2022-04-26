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
import click

from geh_stream.shared.data_loader import initialize_spark
from geh_stream.snapshot import create_snapshot


@click.command()
# Business settings
@click.option('--job-id', type=str, required=True)
@click.option('--grid-area', type=str, required=False,
          help='Run aggregation for specific grid areas format is { "areas": ["123","234"]}. If none is specifed. All grid areas are calculated')
@click.option('--beginning-date-time', type=str, required=True,
          help='The timezone aware date-time representing the beginning of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
@click.option('--end-date-time', type=str, required=True,
          help='The timezone aware date-time representing the end of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
@click.option('--snapshot-id', type=str, required=True)
# Infrastructure settings
@click.option('--snapshot-notify-url', type=str, required=True, help="The target url to post result json")
@click.option('--snapshots-base-path', type=str, required=True)
@click.option('--time-series-points-delta-table-name', type=str, required=True, default="time-series-points", help='The time series points Delta table name')
@click.option('--shared-storage-account-name', type=str, required=True, help='Shared Azure Storage account name holding time series data')
@click.option('--shared-storage-account-key', type=str, required=True, help='Shared Azure Storage key for storage')
@click.option('--shared-storage-time-series-base-path', type=str, required=True, default='data', help='Shared Azure Storage time-series base path including container name')
@click.option('--shared-storage-aggregations-base-path', type=str, required=True, default='data', help='Shared Azure Storage aggregations base path including container name')
@click.option('--shared-database-url', type=str, required=True)
@click.option('--shared-database-aggregations', type=str, required=True)
@click.option('--shared-database-username', type=str, required=True)
@click.option('--shared-database-password', type=str, required=True)
@click.option('--only-validate-params', is_flag=True, required=False, default=False, help='Set to True to exit after validating job parameters')
def run(grid_area, **kwargs):
    """Prepare job that creates snapshots for aggregations and wholesale calculations."""
    if kwargs["only_validate_params"]:
        exit(0)

    areas = []
    if grid_area:
        areasParsed = json.loads(grid_area)
        areas = areasParsed["areas"]

    class Namespace:
        def __init__(self, **kwargs):
            self.__dict__.update(kwargs)

    args = Namespace(**kwargs)
    spark = initialize_spark(args)

    create_snapshot(spark, areas, args)


if __name__ == '__main__':
    run()
