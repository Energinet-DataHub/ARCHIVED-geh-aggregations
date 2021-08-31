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

import configargparse


def trigger_base_arguments():
    p = configargparse.ArgParser(description='Green Energy Hub Tempory aggregation triggger', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)
    p.add('--data-storage-account-name', type=str, required=False, help='Azure Storage account name holding time series data')
    p.add('--data-storage-account-key', type=str, required=False, help='Azure Storage key for storage', env_var='GEH_INPUT_STORAGE_KEY')
    p.add('--data-storage-container-name', type=str, required=False, default='data', help='Azure Storage container name for input storage')
    p.add('--time-series-path', type=str, required=False, default="delta/time-series-test-data/", help='Path to time series data storage location (deltalake) relative to root container')
    p.add('--beginning-date-time', type=str, required=False, help='The timezone aware date-time representing the beginning of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
    p.add('--end-date-time', type=str, required=False, help='The timezone aware date-time representing the end of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
    p.add('--grid-area', type=str, required=False, help='Run aggregation for specific grid areas format is { "areas": ["123","234"]}. If none is specifed. All grid areas are calculated')
    p.add('--process-type', type=str, required=False, help='D03 (Aggregation) or D04 (Balance fixing) '),
    p.add('--result-url', type=str, required=False, help="The target url to post result json"),
    p.add('--result-id', type=str, required=True, help="Postback id that will be added to header. The id is unique"),
    p.add('--persist-source-dataframe', type=bool, required=False, default=False)
    p.add('--persist-source-dataframe-location', type=str, required=False, default="delta/basis-data/")
    p.add('--snapshot-url', type=str, required=False, help="The target url to post result json")
    p.add('--cosmos-account-endpoint', type=str, required=False, help="Cosmos account endpoint")
    p.add('--cosmos-account-key', type=str, required=False, help="Cosmos account key")
    p.add('--cosmos-database', type=str, required=False, help="Cosmos database name")
    p.add('--cosmos-container-metering-points', type=str, required=False, help="Cosmos container for metering points input data")
    p.add('--cosmos-container-market-roles', type=str, required=False, help="Cosmos container for market roles input data")
    p.add('--cosmos-container-grid-loss-sys-corr', type=str, required=False, help="Cosmos container for grid loss and system correction")
    p.add('--cosmos-container-es-brp-relations', type=str, required=False, help="Cosmos container for relations between energy supplier and balance responsible")
    p.add('--cosmos-container-charges', type=str, required=False, help="Cosmos container for charges input data")
    p.add('--cosmos-container-charge-links', type=str, required=False, help="Cosmos container for charge links input data")
    p.add('--cosmos-container-charge-prices', type=str, required=False, help="Cosmos container for charge prices input data")
    return p
