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
from geh_stream.aggregation_utils.trigger_base_arguments import trigger_base_arguments
from geh_stream.shared.data_loader import initialize_spark
from geh_stream.codelists.resolution_duration import ResolutionDuration
from geh_stream.wholesale_utils.wholesale_initializer import get_tariff_charges, get_fee_charges, get_subscription_charges
from geh_stream.shared.services import InputOutputProcessor
from geh_stream.wholesale_utils.calculators import calculate_daily_subscription_price, calculate_tariff_price_per_ga_co_es, calculate_fee_charge_price
from geh_stream.codelists import BasisDataKeyName, ResultKeyName
from geh_stream.shared.data_exporter import export_to_csv

p = trigger_base_arguments()

args, unknown_args = p.parse_known_args()

if unknown_args:
    print("Unknown args: {0}".format(args))

spark = initialize_spark(args.data_storage_account_name, args.data_storage_account_key)

io_processor = InputOutputProcessor(args)

time_series = io_processor.load_basis_data(spark, BasisDataKeyName.time_series)
charges = io_processor.load_basis_data(spark, BasisDataKeyName.charges)
charge_links = io_processor.load_basis_data(spark, BasisDataKeyName.charge_links)
charge_prices = io_processor.load_basis_data(spark, BasisDataKeyName.charge_prices)
metering_points = io_processor.load_basis_data(spark, BasisDataKeyName.metering_points)
market_roles = io_processor.load_basis_data(spark, BasisDataKeyName.market_roles)

# Initialize wholesale specific data frames
daily_tariff_charges = get_tariff_charges(time_series, charges, charge_links, charge_prices, metering_points, market_roles, ResolutionDuration.day)

hourly_tariff_charges = get_tariff_charges(time_series, charges, charge_links, charge_prices, metering_points, market_roles, ResolutionDuration.hour)

fee_charges = get_fee_charges(charges, charge_prices, charge_links, metering_points, market_roles)

subscription_charges = get_subscription_charges(charges, charge_prices, charge_links, metering_points, market_roles)

# Create a keyvalue dictionary for use in postprocessing. Each result are stored as a keyval with value being dataframe
results = {}

results[ResultKeyName.hourly_tariffs] = calculate_tariff_price_per_ga_co_es(
    spark,
    hourly_tariff_charges)

results[ResultKeyName.daily_tariffs] = calculate_tariff_price_per_ga_co_es(
    spark,
    daily_tariff_charges)

results[ResultKeyName.subscription_prices] = calculate_daily_subscription_price(
    spark,
    subscription_charges)

results[ResultKeyName.fee_prices] = calculate_fee_charge_price(
    spark,
    fee_charges)

# Enable to dump results to local csv files
# export_to_csv(results)

# Store wholesale results
io_processor.do_post_processing(args.process_type, args.job_id, args.result_url, results)
