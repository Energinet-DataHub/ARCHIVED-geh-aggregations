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
from geh_stream.codelists import Colname, MarketEvaluationPointType, SettlementMethod, ChargeType
from geh_stream.schemas.output import calculate_daily_subscription_price_schema


def calculate_daily_subscription_price(spark: SparkSession, charges: DataFrame, charge_links: DataFrame, charge_prices: DataFrame, metering_points: DataFrame, market_roles: DataFrame) -> DataFrame:
    # Only look at subcriptions of type D01
    subscription_charge_type = ChargeType.subscription
    subscription_charges = charges.filter(col(Colname.charge_type) == subscription_charge_type) \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.from_date,
            Colname.to_date
        )

    # Join charges and charge_prices
    charges_with_prices = charge_prices \
        .join(subscription_charges, [Colname.charge_key], "inner") \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.from_date,
            Colname.to_date,
            Colname.time,
            Colname.charge_price
        )

    # Create new colum with price per day of 'time' columns month
    charges_with_price_per_day = charges_with_prices.withColumn(Colname.price_per_day, (col(Colname.charge_price) / dayofmonth(last_day(col(Colname.time)))).cast(DecimalType(14, 8))) \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.from_date,
            Colname.to_date,
            Colname.time,
            Colname.charge_price,
            Colname.price_per_day
        )

    # Explode dataframe: create row for each day the time period from and to date
    charges_with_price_per_day_exploded = charges_with_price_per_day.withColumn(Colname.date, explode(expr("sequence(from_date, to_date, interval 1 day)"))) \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price,
            Colname.price_per_day,
            Colname.date
        ) \
        .filter((month(Colname.date) == month(Colname.time)))

    # Explode dataframe: create row for each day the time period from and to date
    charge_links_exploded = charge_links.withColumn(Colname.date, explode(expr("sequence(from_date, to_date, interval 1 day)"))) \
        .select(
            Colname.charge_key,
            Colname.metering_point_id,
            Colname.date
        )

    # Join the two exploded dataframes on charge_key and the new column date
    charges_with_price_per_day_and_links = charges_with_price_per_day_exploded.join(charge_links_exploded, [Colname.charge_key, Colname.date], "inner") \
        .select(
            Colname.charge_key,
            Colname.metering_point_id,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price,
            Colname.price_per_day,
            Colname.date
        )

    charges_per_day_with_metering_point_join_condition = [
        charges_with_price_per_day_and_links[Colname.metering_point_id] == metering_points[Colname.metering_point_id],
        charges_with_price_per_day_and_links[Colname.date] >= metering_points[Colname.from_date],
        charges_with_price_per_day_and_links[Colname.date] < metering_points[Colname.to_date]
    ]

    charges_per_day_with_metering_point = charges_with_price_per_day_and_links.join(metering_points, charges_per_day_with_metering_point_join_condition, "inner") \
        .select(
            Colname.charge_key,
            metering_points[Colname.metering_point_id],
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price,
            Colname.price_per_day,
            Colname.date,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.connection_state
        )

    charges_per_day_with_metering_point_and_energy_supplier_join_condition = [
        charges_per_day_with_metering_point[Colname.metering_point_id] == market_roles[Colname.metering_point_id],
        charges_per_day_with_metering_point[Colname.date] >= market_roles[Colname.from_date],
        charges_per_day_with_metering_point[Colname.date] < market_roles[Colname.to_date]
    ]

    charges_per_day_with_metering_point_and_energy_supplier = charges_per_day_with_metering_point.join(market_roles, charges_per_day_with_metering_point_and_energy_supplier_join_condition, "inner") \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price,
            Colname.price_per_day,
            Colname.date,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.connection_state,
            Colname.energy_supplier_id
        )

    charges_per_day_flex_settled_consumption = charges_per_day_with_metering_point_and_energy_supplier \
        .filter(col(Colname.metering_point_type) == MarketEvaluationPointType.consumption.value) \
        .filter(col(Colname.settlement_method) == SettlementMethod.flex_settled.value)

    grouped_charges_per_day = charges_per_day_flex_settled_consumption \
        .groupBy(Colname.charge_owner, Colname.grid_area, Colname.energy_supplier_id, Colname.date) \
        .agg(
            count("*").alias(Colname.charge_count),
            sum(Colname.price_per_day).alias(Colname.total_daily_charge_price)
            ) \
        .select(
            Colname.charge_owner,
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.date,
            Colname.charge_count,
            Colname.total_daily_charge_price
        )

    df = charges_per_day_flex_settled_consumption \
        .select("*").distinct().join(grouped_charges_per_day, [Colname.charge_owner, Colname.grid_area, Colname.energy_supplier_id, Colname.date], "inner") \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_price,
            Colname.date,
            Colname.price_per_day,
            Colname.charge_count,
            Colname.total_daily_charge_price,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.connection_state,
            Colname.energy_supplier_id
        )

    return spark.createDataFrame(df.rdd, calculate_daily_subscription_price_schema)
