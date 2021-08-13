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
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, expr, year, month, lit, last_day, dayofmonth, explode
from pyspark.sql.types import IntegerType
from geh_stream.codelists import Colname
from geh_stream.shared.data_exporter import export_to_csv
from calendar import monthrange


price_per_day = "price_per_day"
date = "date"


def calculate_daily_subscription_price(charges: DataFrame, charge_links: DataFrame, charge_prices: DataFrame, metering_points: DataFrame, market_roles: DataFrame, time_series: DataFrame):
    # Only look at subcriptions D01
    subscription_charge_type = "D01"
    subscription_charges = charges.filter(col(Colname.charge_type) == subscription_charge_type) \
        .selectExpr(
            Colname.charge_key,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.from_date,
            Colname.to_date
        )

    # Join charges and charge_prices
    charges_with_prices = charge_prices \
        .join(subscription_charges, [Colname.charge_key]) \
        .selectExpr(
            Colname.charge_key,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.from_date,
            Colname.to_date,
            Colname.time,
            Colname.charge_price
        )

    # Create new colum with price per day of 'time' columns month
    charges_with_price_per_day = charges_with_prices.withColumn(price_per_day, (col(Colname.charge_price)/dayofmonth(last_day(col(Colname.time))))) \
        .selectExpr(
            Colname.charge_key,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.from_date,
            Colname.to_date,
            Colname.time,
            Colname.charge_price,
            price_per_day
        )

    # Explode dataframe: create row for each day the time period from and to date
    charges_with_price_per_day_exploded = charges_with_price_per_day.withColumn(date, explode(expr("sequence(from_date, to_date, interval 1 day)"))) \
        .selectExpr(
            Colname.charge_key,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price,
            price_per_day,
            date
        )
    
    # Explode dataframe: create row for each day the time period from and to date
    charge_links_exploded = charge_links.withColumn(date, explode(expr("sequence(from_date, to_date, interval 1 day)"))) \
        .selectExpr(
            Colname.charge_key,
            Colname.metering_point_id,
            date
        )
    
    # Join the two exploded dataframes on charge_key and the new column date
    charges_with_price_per_day_and_links = charges_with_price_per_day_exploded.join(charge_links_exploded, [Colname.charge_key, date]) \
        .selectExpr(
            Colname.charge_key,
            Colname.metering_point_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price,
            price_per_day,
            date
        )
    
    charges_per_day_with_metering_point_join_condition = [
        charges_with_price_per_day_and_links[Colname.metering_point_id] == metering_points[Colname.metering_point_id],
        charges_with_price_per_day_and_links[date] >= metering_points[Colname.from_date],
        charges_with_price_per_day_and_links[date] < metering_points[Colname.to_date]
    ]

    charges_per_day_with_metering_point = charges_with_price_per_day_and_links.join(metering_points, charges_per_day_with_metering_point_join_condition) \
        .select(
            Colname.charge_key,
            metering_points[Colname.metering_point_id],
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price,
            price_per_day,
            date,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.connection_state
        )
    charges_per_day_with_metering_point.show(1000, False)

    charges_with_price_per_day_and_links.show(1000, False)


