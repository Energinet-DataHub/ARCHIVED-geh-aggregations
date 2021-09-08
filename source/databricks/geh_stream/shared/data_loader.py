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
from argparse import Namespace
from pyspark.sql.dataframe import DataFrame
from geh_stream.schemas import metering_point_schema, grid_loss_sys_corr_schema, market_roles_schema, charges_schema, charge_links_schema, charge_prices_schema, es_brp_relations_schema
from pyspark import SparkConf
from pyspark.sql.session import SparkSession
from pyspark.sql.types import StructType
from geh_stream.shared.filters import filter_on_date, filter_on_period, filter_on_grid_areas, time_series_where_date_condition
from typing import List
from geh_stream.shared.services import StorageAccountService
from geh_stream.shared.period import parse_period


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


def __load_cosmos_data(cosmos_container_name: str, schema: StructType, args: Namespace, spark: SparkSession) -> DataFrame:
    config = {
        "spark.cosmos.accountEndpoint": args.cosmos_account_endpoint,
        "spark.cosmos.accountKey": args.cosmos_account_key,
        "spark.cosmos.database": args.cosmos_database,
        "spark.cosmos.container": cosmos_container_name,
        "spark.cosmos.read.inferSchema.forceNullableProperties": False
    }
    return spark.read.schema(schema).format("cosmos.oltp").options(**config).load()


def __load_delta_data(spark: SparkSession, storage_container_name: str, storage_account_name: str, delta_table_path: str, where_condition: str = None) -> DataFrame:
    path = StorageAccountService.get_storage_account_full_path(storage_container_name, storage_account_name, delta_table_path)
    df = spark \
        .read \
        .format("delta") \
        .load(path)

    if where_condition is not None:
        df = df.where(where_condition)

    return df


def load_metering_points(args: Namespace, spark: SparkSession, grid_areas: List[str]) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_metering_points, metering_point_schema, args, spark)
    df = filter_on_period(df, parse_period(args))
    df = filter_on_grid_areas(df, Colname.grid_area, grid_areas)
    return df


def load_grid_loss_sys_corr(args: Namespace, spark: SparkSession, grid_areas: List[str]) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_grid_loss_sys_corr, grid_loss_sys_corr_schema, args, spark)
    df = filter_on_period(df, parse_period(args))
    df = filter_on_grid_areas(df, Colname.grid_area, grid_areas)
    return df


def load_market_roles(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_market_roles, market_roles_schema, args, spark)
    return filter_on_period(df, parse_period(args))


def load_charges(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_charges, charges_schema, args, spark)
    return filter_on_period(df, parse_period(args))


def load_charge_links(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_charge_links, charge_links_schema, args, spark)
    return filter_on_period(df, parse_period(args))


def load_charge_prices(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_charge_prices, charge_prices_schema, args, spark)
    df = filter_on_date(df, parse_period(args))
    return df


def load_es_brp_relations(args: Namespace, spark: SparkSession, grid_areas: List[str]) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_es_brp_relations, es_brp_relations_schema, args, spark)
    df = filter_on_period(df, parse_period(args))
    return filter_on_grid_areas(df, Colname.grid_area, grid_areas)


def load_time_series(args: Namespace, spark: SparkSession, grid_areas: List[str]) -> DataFrame:
    df = __load_delta_data(spark, args.data_storage_container_name, args.data_storage_account_name, args.time_series_path, time_series_where_date_condition(parse_period(args)))
    df = filter_on_date(df, parse_period(args))
    df = filter_on_grid_areas(df, Colname.grid_area, grid_areas)
    return df
