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
from pyspark.sql.functions import col, last_day, dayofmonth, count, sum
from pyspark.sql.types import DecimalType
from geh_stream.codelists import Colname, MarketEvaluationPointType, SettlementMethod, ChargeType
from geh_stream.schemas.output import calculate_daily_subscription_price_schema
from .shared_fee_and_subscription import join_properties_on_charges_with_given_charge_type


def calculate_daily_subscription_price(spark: SparkSession, charges: DataFrame, charge_links: DataFrame, charge_prices: DataFrame, metering_points: DataFrame, market_roles: DataFrame) -> DataFrame:
    charges_for_subscription = join_properties_on_charges_with_given_charge_type(charges, charge_prices, charge_links, metering_points, market_roles, ChargeType.subscription)

    charges_per_day_flex_settled_consumption = charges_for_subscription \
        .filter(col(Colname.metering_point_type) == MarketEvaluationPointType.consumption.value) \
        .filter(col(Colname.settlement_method) == SettlementMethod.flex_settled.value)

    charges_per_day = charges_per_day_flex_settled_consumption.withColumn(Colname.price_per_day, (col(Colname.charge_price) / dayofmonth(last_day(col(Colname.time)))).cast(DecimalType(14, 8)))

    grouped_charges_per_day = charges_per_day \
        .groupBy(Colname.charge_owner, Colname.grid_area, Colname.energy_supplier_id, Colname.time) \
        .agg(
            count("*").alias(Colname.charge_count),
            sum(Colname.price_per_day).alias(Colname.total_daily_charge_price)
            ) \
        .select(
            Colname.charge_owner,
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.time,
            Colname.charge_count,
            Colname.total_daily_charge_price
        )

    df = charges_per_day \
        .select("*").distinct().join(grouped_charges_per_day, [Colname.charge_owner, Colname.grid_area, Colname.energy_supplier_id, Colname.time]) \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_price,
            Colname.time,
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
