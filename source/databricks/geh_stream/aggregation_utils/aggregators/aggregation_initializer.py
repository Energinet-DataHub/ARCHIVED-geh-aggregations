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

from geh_stream.codelists import Colname
from pyspark import SparkConf
from pyspark.sql import SparkSession
from pyspark.sql.functions import col
from geh_stream.aggregation_utils.filters import filter_time_period
from geh_stream.schemas import metering_point_schema, grid_loss_sys_corr_schema, market_roles_schema, charges_schema, charge_links_schema, charge_prices_schema, es_brp_relations_schema
import dateutil.parser


def initialize_spark(args):
    # Set spark config with storage account names/keys and the session timezone so that datetimes are displayed consistently (in UTC)
    spark_conf = SparkConf(loadDefaults=True) \
        .set('fs.azure.account.key.{0}.dfs.core.windows.net'.format(args.data_storage_account_name), args.data_storage_account_key) \
        .set("spark.sql.session.timeZone", "UTC") \
        .set("spark.databricks.io.cache.enabled", "True")

    return SparkSession \
        .builder\
        .config(conf=spark_conf)\
        .getOrCreate()


def load_metering_points(args, spark):
    return load_aggregation_data(args.cosmos_container_metering_points, metering_point_schema, args, spark)


def load_grid_loss_sys_corr(args, spark):
    return load_aggregation_data(args.cosmos_container_grid_loss_sys_corr, grid_loss_sys_corr_schema, args, spark)


def load_market_roles(args, spark):
    return load_aggregation_data(args.cosmos_container_market_roles, market_roles_schema, args, spark)


def load_charges(args, spark):
    return load_aggregation_data(args.cosmos_container_charges, charges_schema, args, spark)


def load_charge_links(args, spark):
    return load_aggregation_data(args.cosmos_container_charge_links, charge_links_schema, args, spark)


def load_charge_prices(args, spark):
    return load_aggregation_data(args.cosmos_container_charge_prices, charge_prices_schema, args, spark)


def load_es_brp_relations(args, spark):
    return load_aggregation_data(args.cosmos_container_es_brp_relations, es_brp_relations_schema, args, spark)


def load_aggregation_data(cosmos_container_name, schema, args, spark):
    config = {
        "spark.cosmos.accountEndpoint": args.cosmos_account_endpoint,
        "spark.cosmos.accountKey": args.cosmos_account_key,
        "spark.cosmos.database": args.cosmos_database,
        "spark.cosmos.container": cosmos_container_name,
        "spark.cosmos.read.inferSchema.forceNullableProperties": False
    }
    return spark.read.schema(schema).format("cosmos.oltp").options(**config).load()


def get_translated_grid_loss_sys_corr(args, spark):
    return load_grid_loss_sys_corr(args, spark)


def get_time_series_dataframe(args, areas, spark):
    time_series_df = load_time_series(args, areas, spark)
    metering_point_df = load_metering_points(args, spark)
    market_roles_df = load_market_roles(args, spark)
    # charges_df = load_charges(args, spark)
    # charge_links_df = load_charge_links(args, spark)
    # charge_prices_df = load_charge_prices(args, spark)
    es_brp_relations_df = load_es_brp_relations(args, spark)

    metering_point_join_conditions = \
        [
            time_series_df.metering_point_id == metering_point_df.metering_point_id,
            time_series_df.time >= metering_point_df.from_date,
            time_series_df.time < metering_point_df.to_date
        ]

    time_series_with_metering_point = time_series_df \
        .join(metering_point_df, metering_point_join_conditions) \
        .drop(metering_point_df.metering_point_id) \
        .drop(metering_point_df.from_date) \
        .drop(metering_point_df.to_date)

    market_roles_join_conditions = \
        [
            time_series_with_metering_point.metering_point_id == market_roles_df.metering_point_id,
            time_series_with_metering_point.time >= market_roles_df.from_date,
            time_series_with_metering_point.time < market_roles_df.to_date
        ]

    time_series_with_metering_point_and_market_roles = time_series_with_metering_point \
        .join(market_roles_df, market_roles_join_conditions, "left") \
        .drop(market_roles_df.metering_point_id) \
        .drop(market_roles_df.from_date) \
        .drop(market_roles_df.to_date)

    es_brp_relations_join_conditions = \
        [
            time_series_with_metering_point_and_market_roles.energy_supplier_id == es_brp_relations_df.energy_supplier_id,
            time_series_with_metering_point_and_market_roles.grid_area == es_brp_relations_df.grid_area,
            time_series_with_metering_point_and_market_roles.metering_point_type == es_brp_relations_df.type_of_metering_point,
            time_series_with_metering_point_and_market_roles.time >= es_brp_relations_df.from_date,
            time_series_with_metering_point_and_market_roles.time < es_brp_relations_df.to_date,
            time_series_with_metering_point_and_market_roles.metering_point_type == es_brp_relations_df.metering_point_type
        ]

    time_series_with_metering_point_and_market_roles_and_brp = time_series_with_metering_point_and_market_roles \
        .join(es_brp_relations_df, es_brp_relations_join_conditions, "left") \
        .drop(es_brp_relations_df.energy_supplier_id) \
        .drop(es_brp_relations_df.grid_area) \
        .drop(es_brp_relations_df.from_date) \
        .drop(es_brp_relations_df.to_date) \
        .drop(es_brp_relations_df.metering_point_type)

    # Add charges for BRS-027
    # charges_with_prices_and_links = charges_df \
    #     .join(charge_prices_df, ["charge_id"], "left") \
    #     .filter((col("time") >= col("from_date"))) \
    #     .filter((col("time") <= col("to_date"))) \
    #     .join(charge_links_df, ["charge_id", "from_date", "to_date"])
    # charges_with_prices_and_links.show()

    # time_series_with_metering_point_and_charges = time_series_with_metering_point \
    #     .join(charges_with_prices_and_links, ["metering_point_id", "from_date", "to_date"])

    return time_series_with_metering_point_and_market_roles_and_brp


def load_time_series(args, areas, spark):
    beginning_date_time = dateutil.parser.parse(args.beginning_date_time)
    end_date_time = dateutil.parser.parse(args.end_date_time)

    TIME_SERIES_STORAGE_PATH = f"abfss://{args.data_storage_container_name}@{args.data_storage_account_name}.dfs.core.windows.net/{args.time_series_path}"

    print("Time series storage url:", TIME_SERIES_STORAGE_PATH)

    beginning_condition = f"Year >= {beginning_date_time.year} AND Month >= {beginning_date_time.month} AND Day >= {beginning_date_time.day}"
    end_condition = f"Year <= {end_date_time.year} AND Month <= {end_date_time.month} AND Day <= {end_date_time.day}"

    # Read in time series data (delta doesn't support user specified schema)
    timeseries_df = spark \
        .read \
        .format("delta") \
        .load(TIME_SERIES_STORAGE_PATH) \
        .where(f"{beginning_condition} AND {end_condition}")
    # Filter out time series data that is not in the specified time period
    valid_time_period_df = filter_time_period(timeseries_df, beginning_date_time, end_date_time)
    # Filter out time series data that do not belong to the specified grid areas
    if areas:
        valid_time_period_df = valid_time_period_df \
            .filter(col("grid_area").isin(areas))

    return valid_time_period_df
