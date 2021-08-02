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

from argparse import Namespace
from pyspark.sql.dataframe import DataFrame
from pyspark.sql.functions import col
from geh_stream.schemas import time_series_schema, metering_point_schema, grid_loss_sys_corr_schema, market_roles_schema, charges_schema, charge_links_schema, charge_prices_schema, es_brp_relations_schema
from pyspark.sql.session import SparkSession
from pyspark.sql.types import StructType
from geh_stream.shared.filters import filter_on_date 


def __load_cosmos_data(cosmos_container_name: str, schema: StructType, args: Namespace, spark: SparkSession) -> DataFrame:
    config = {
        "spark.cosmos.accountEndpoint": args.cosmos_account_endpoint,
        "spark.cosmos.accountKey": args.cosmos_account_key,
        "spark.cosmos.database": args.cosmos_database,
        "spark.cosmos.container": cosmos_container_name,
        "spark.cosmos.read.inferSchema.forceNullableProperties": False
    }
    return spark.read.schema(schema).format("cosmos.oltp").options(**config).load()

def load_metering_points(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_metering_points, metering_point_schema, args, spark)
    return filter_on_date(df, "from_date", "to_date", args)


def load_grid_loss_sys_corr(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_grid_loss_sys_corr, grid_loss_sys_corr_schema, args, spark)
    return filter_on_date(df, "from_date", "to_date", args)


def load_market_roles(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_market_roles, market_roles_schema, args, spark)
    return filter_on_date(df, "from_date", "to_date", args)


def load_charges(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_charges, charges_schema, args, spark)
    return filter_on_date(df, "from_date", "to_date", args)


def load_charge_links(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_charge_links, charge_links_schema, args, spark)
    return filter_on_date(df, "from_date", "to_date", args)


def load_charge_prices(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_charge_prices, charge_prices_schema, args, spark)
    return filter_on_date(df, "from_date", "to_date", args)


def load_es_brp_relations(args: Namespace, spark: SparkSession) -> DataFrame:
    df = __load_cosmos_data(args.cosmos_container_es_brp_relations, es_brp_relations_schema, args, spark)
    return filter_on_date(df, "from_date", "to_date", args)