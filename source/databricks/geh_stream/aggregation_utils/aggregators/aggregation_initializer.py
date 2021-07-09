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
from geh_stream.schemas import metering_point_schema, grid_loss_sys_corr_schema, market_roles_schema, charges_schema, charge_links_schema, charge_prices_schema, es_brp_relations_schema
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


def load_es_brp_relations(args, spark):
    return load_aggregation_data("es-brp-relations", es_brp_relations_schema, args, spark)


def load_aggregation_data(cosmos_container_name, schema, args, spark):
    config = {
        "spark.cosmos.accountEndpoint": args.cosmos_account_endpoint,
        "spark.cosmos.accountKey": args.cosmos_account_key,
        "spark.cosmos.database": args.cosmos_database,
        "spark.cosmos.container": cosmos_container_name,
        "spark.cosmos.read.inferSchema.forceNullableProperties": False
    }
    return spark.read.schema(schema).format("cosmos.oltp").options(**config).load()


def get_time_series_dataframe(args, areas, spark):
    time_series_df = load_time_series(args, areas, spark)
    metering_point_df = load_metering_points(args, spark)
    market_roles_df = load_market_roles(args, spark)
    grid_loss_sys_corr_df = load_grid_loss_sys_corr(args, spark)
    charges_df = load_charges(args, spark)
    charge_links_df = load_charge_links(args, spark)
    charge_prices_df = load_charge_prices(args, spark)
    es_brp_relations_df = load_es_brp_relations(args, spark)

    print("time_series_df = " + str(time_series_df.count()))

    # metering_point_df = metering_point_df.filter(metering_point_df.metering_point_type == "E17")

    time_serie_with_metering_point = time_series_df \
        .join(metering_point_df, ["metering_point_id"]) \
        .filter((col("time") >= col("from_date"))) \
        .filter((col("time") < col("to_date"))) \
        .drop("from_date") \
        .drop("to_date")
    time_serie_with_metering_point.show()
    time_serie_with_metering_point.coalesce(1).write.option("sep",",").option("header","true").mode('overwrite').csv("time_serie_with_metering_point.csv")
    print("time_serie_with_metering_point = " + str(time_serie_with_metering_point.count()))


    time_serie_with_metering_point_and_market_roles = time_serie_with_metering_point \
        .join(market_roles_df, ["metering_point_id"]) \
        .orderBy("metering_point_id", "time") \
        .filter((col("time") >= col("from_date"))) \
        .filter((col("time") < col("to_date"))) \
        .drop("from_date") \
        .drop("to_date")
    time_serie_with_metering_point_and_market_roles.show()
    time_serie_with_metering_point_and_market_roles.coalesce(1).write.option("sep","|").option("header","true").mode('overwrite').csv("time_serie_with_metering_point_and_market_roles.csv")
    print("time_serie_with_metering_point_and_market_roles = " + str(time_serie_with_metering_point_and_market_roles.count()))

    conditions = \
        [
            time_serie_with_metering_point_and_market_roles.metering_point_id == grid_loss_sys_corr_df.metering_point_id,
            time_serie_with_metering_point_and_market_roles.energy_supplier_id == grid_loss_sys_corr_df.energy_supplier_id,
            time_serie_with_metering_point_and_market_roles.grid_area == grid_loss_sys_corr_df.grid_area,
            time_serie_with_metering_point_and_market_roles.time >= grid_loss_sys_corr_df.from_date,
            time_serie_with_metering_point_and_market_roles.time < grid_loss_sys_corr_df.to_date
        ]

    time_serie_with_metering_point_and_market_roles_and_grid_loss_sys_corr = time_serie_with_metering_point_and_market_roles \
        .join(grid_loss_sys_corr_df, conditions, "left") \
        .drop(grid_loss_sys_corr_df.metering_point_id) \
        .drop(grid_loss_sys_corr_df.energy_supplier_id) \
        .drop(grid_loss_sys_corr_df.grid_area) \
        .drop(grid_loss_sys_corr_df.from_date) \
        .drop(grid_loss_sys_corr_df.to_date)

    # time_serie_with_metering_point_and_market_roles_and_grid_loss_sys_corr = time_serie_with_metering_point_and_market_roles \
    #     .join(grid_loss_sys_corr_df, ["metering_point_id", "energy_supplier_id", "grid_area"], how="left") \
    #     .filter((col("time") >= col("from_date"))) \
    #     .filter((col("time") < col("to_date"))) \
    #     .drop("from_date") \
    #     .drop("to_date")
    time_serie_with_metering_point_and_market_roles_and_grid_loss_sys_corr.show()
    time_serie_with_metering_point_and_market_roles_and_grid_loss_sys_corr.coalesce(1).write.option("sep","|").option("header","true").mode('overwrite').csv("time_serie_with_metering_point_and_market_roles_and_grid_loss_sys_corr.csv")
    print("time_serie_with_metering_point_and_market_roles_and_grid_loss_sys_corr = " + str(time_serie_with_metering_point_and_market_roles_and_grid_loss_sys_corr.count()))
    time_serie_with_metering_point_and_market_roles_and_grid_loss_sys_corr.printSchema()

    # Add charges for BRS-027
    # charges_with_prices_and_links = charges_df \
    #     .join(charge_prices_df, ["charge_id"], "left") \
    #     .filter((col("time") >= col("from_date"))) \
    #     .filter((col("time") <= col("to_date"))) \
    #     .join(charge_links_df, ["charge_id", "from_date", "to_date"])
    # charges_with_prices_and_links.show()

    # time_serie_with_metering_point_and_charges = time_serie_with_metering_point \
    #     .join(charges_with_prices_and_links, ["metering_point_id", "from_date", "to_date"])
    # time_serie_with_metering_point_and_charges.show()


    translated = time_serie_with_metering_point_and_market_roles_and_grid_loss_sys_corr \
        .withColumnRenamed("metering_point_id", "MarketEvaluationPoint_mRID") \
        .withColumnRenamed("time", "Time") \
        .withColumnRenamed("resolution", "MeterReadingPeriodicity") \
        .withColumnRenamed("metering_method", "MeteringMethod") \
        .withColumnRenamed("grid_area", "MeteringGridArea_Domain_mRID") \
        .withColumnRenamed("connection_state", "ConnectionState") \
        .withColumnRenamed("metering_point_type", "MarketEvaluationPointType") \
        .withColumnRenamed("energy_supplier_id", "EnergySupplier_MarketParticipant_mRID") \
        .withColumnRenamed("in_grid_area", "InMeteringGridArea_Domain_mRID") \
        .withColumnRenamed("out_grid_area", "OutMeteringGridArea_Domain_mRID") \
        .withColumnRenamed("settlement_method", "SettlementMethod") \
        .withColumnRenamed("product", "Product") \
        .withColumnRenamed("quantity", "Quantity") \
        .withColumnRenamed("quality", "Quality")
        # .withColumnRenamed("balance_responsible_id", "BalanceResponsibleParty_MarketParticipant_mRID")
        # .withColumnRenamed("net_settlement_group", "?") \ #brs 27
        # .withColumnRenamed("?", "CreatedDateTime") \

    translated.show()
    return translated


def load_time_series(args, areas, spark):
    beginning_date_time = dateutil.parser.parse(args.beginning_date_time)
    end_date_time = dateutil.parser.parse(args.end_date_time)

    INPUT_STORAGE_PATH = "abfss://{0}@{1}.dfs.core.windows.net/{2}".format(
        args.input_storage_container_name, args.input_storage_account_name, args.input_path
    )

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
            .filter(col("metering_point_id").isin(areas))

    return valid_time_period_df
