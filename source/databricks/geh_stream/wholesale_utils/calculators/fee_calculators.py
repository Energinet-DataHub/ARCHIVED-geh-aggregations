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
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col, expr, last_day, dayofmonth, explode, count, sum, month
from pyspark.sql.types import DecimalType
from geh_stream.codelists import Colname, MarketEvaluationPointType, SettlementMethod
from geh_stream.schemas.output import calculate_daily_subscription_price_schema


def calculate_fee_charge_price(spark: SparkSession, charges: DataFrame, charge_links: DataFrame, charge_prices: DataFrame, metering_points: DataFrame, market_roles: DataFrame) -> DataFrame:
    # Only look at fee of type D02
    fee_charge_type = "D02"
    fee_charges = charges.filter(col(Colname.charge_type) == fee_charge_type) \
        .selectExpr(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.from_date,
            Colname.to_date
        )

    # Join charges and charge_prices
    charges_with_prices = charge_prices \
        .join(fee_charges, [Colname.charge_key]) \
        .selectExpr(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.from_date,
            Colname.to_date,
            Colname.time,
            Colname.charge_price
        )

    charges_with_price_and_links_join_condition = [
        charges_with_prices[Colname.charge_key] == charge_links[Colname.charge_key],
        charges_with_prices[Colname.time] >= charge_links[Colname.from_date],
        charges_with_prices[Colname.time] < charge_links[Colname.to_date]
    ]
    # Join the two exploded dataframes on charge_key and the new column date
    charges_with_price_and_links = charges_with_prices.join(charge_links, charges_with_price_and_links_join_condition) \
        .selectExpr(
            Colname.charge_key,
            Colname.metering_point_id,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price
        )
    charges_with_price_and_links.show(1000, False)

    charges_with_metering_point_join_condition = [
        charges_with_price_and_links[Colname.metering_point_id] == metering_points[Colname.metering_point_id],
        charges_with_price_and_links[Colname.time] >= metering_points[Colname.from_date],
        charges_with_price_and_links[Colname.time] < metering_points[Colname.to_date]
    ]

    charges_with_metering_point = charges_with_price_and_links.join(metering_points, charges_with_metering_point_join_condition) \
        .select(
            Colname.charge_key,
            metering_points[Colname.metering_point_id],
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.connection_state
        )

    charges_with_metering_point_and_energy_supplier_join_condition = [
        charges_with_metering_point[Colname.metering_point_id] == market_roles[Colname.metering_point_id],
        charges_with_metering_point[Colname.time] >= market_roles[Colname.from_date],
        charges_with_metering_point[Colname.time] < market_roles[Colname.to_date]
    ]

    charges_with_metering_point_and_energy_supplier = charges_with_metering_point.join(market_roles, charges_with_metering_point_and_energy_supplier_join_condition) \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.connection_state,
            Colname.energy_supplier_id
        )

    charges_flex_settled_consumption = charges_with_metering_point_and_energy_supplier \
        .filter(col(Colname.metering_point_type) == MarketEvaluationPointType.consumption.value) \
        .filter(col(Colname.settlement_method) == SettlementMethod.flex_settled.value)

    grouped_charges = charges_flex_settled_consumption \
        .groupBy(Colname.charge_owner, Colname.grid_area, Colname.energy_supplier_id, Colname.date) \
        .agg(
            count("*").alias("fee_count"),
            sum(Colname.charge_price).alias("total_fee_price")
            ) \
        .select(
            Colname.charge_owner,
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.time,
            "fee_count",
            "total_fee_price"
        )

    df = charges_flex_settled_consumption \
        .select("*").distinct().join(grouped_charges, [Colname.charge_owner, Colname.grid_area, Colname.energy_supplier_id, Colname.date]) \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_price,
            Colname.time,
            "fee_count",
            "total_fee_price",
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.connection_state,
            Colname.energy_supplier_id
        )
    df.show(1000, False)
    return df
    # return spark.createDataFrame(df.rdd, calculate_daily_subscription_price_schema)
