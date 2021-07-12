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

from pyspark import SparkConf
from pyspark.sql import SparkSession
from pyspark.sql.functions import col
from geh_stream.aggregation_utils.filters import filter_time_period
from geh_stream.schemas import metering_point_schema, grid_loss_sys_corr_schema, market_roles_schema, charges_schema, charge_links_schema, charge_prices_schema
import dateutil.parser


def initialize_spark(args):
    # Set spark config with storage account names/keys and the session timezone so that datetimes are displayed consistently (in UTC)
    spark_conf = SparkConf(loadDefaults=True) \
        .set('fs.azure.account.key.{0}.dfs.core.windows.net'.format(args.input_storage_account_name), args.input_storage_account_key) \
        .set("spark.sql.session.timeZone", "UTC") \
        .set("spark.databricks.io.cache.enabled", "True")

    return SparkSession \
        .builder\
        .config(conf=spark_conf)\
        .getOrCreate()


def load_metering_points(args, spark):
    return load_aggregation_data("metering-points", metering_point_schema, args, spark)


def load_grid_loss_sys_corr(args, spark):
    return load_aggregation_data("grid-loss-sys-corr", grid_loss_sys_corr_schema, args, spark)


def load_market_roles(args, spark):
    return load_aggregation_data("market-roles", market_roles_schema, args, spark)


def load_charges(args, spark):
    return load_aggregation_data("charges", charges_schema, args, spark)


def load_charge_links(args, spark):
    return load_aggregation_data("charge-links", charge_links_schema, args, spark)


def load_charge_prices(args, spark):
    return load_aggregation_data("charge-prices", charge_prices_schema, args, spark)


def load_aggregation_data(cosmos_container_name, schema, args, spark):
    config = {
        "spark.cosmos.accountEndpoint": args.cosmos_account_endpoint,
        "spark.cosmos.accountKey": args.cosmos_account_key,
        "spark.cosmos.database": args.cosmos_database,
        "spark.cosmos.container": cosmos_container_name,
    }
    return spark.read.schema(schema).format("cosmos.oltp").options(**config).load()


def load_timeseries_dataframe(args, areas, spark):
    beginning_date_time = dateutil.parser.parse(args.beginning_date_time)
    end_date_time = dateutil.parser.parse(args.end_date_time)

    # Uncomment to get some info on our spark context
    # sc = spark.sparkContext
    # print("Spark Configuration:")
    # _ = [print(k + '=' + v) for k, v in sc.getConf().getAll()]

    # Create input and output storage paths
    INPUT_STORAGE_PATH = "abfss://{0}@{1}.dfs.core.windows.net/{2}".format(
        args.input_storage_container_name, args.input_storage_account_name, args.input_path
    )

    print("Input storage url:", INPUT_STORAGE_PATH)

    beginning_condition = f"Year >= {beginning_date_time.year} AND Month >= {beginning_date_time.month} AND Day >= {beginning_date_time.day}"
    end_condition = f"Year <= {end_date_time.year} AND Month <= {end_date_time.month} AND Day <= {end_date_time.day}"

    # Read in time series data (delta doesn't support user specified schema)
    timeseries_df = spark \
        .read \
        .format("delta") \
        .load(INPUT_STORAGE_PATH) \
        .where(f"{beginning_condition} AND {end_condition}")

    # Filter out time series data that is not in the specified time period
    valid_time_period_df = filter_time_period(timeseries_df, beginning_date_time, end_date_time)

    # Filter out time series data that do not belong to the specified grid areas
    if areas:
        valid_time_period_df = valid_time_period_df \
            .filter(col("MeteringGridArea_Domain_mRID").isin(areas))

    return valid_time_period_df
