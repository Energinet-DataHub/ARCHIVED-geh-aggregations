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
from pyspark.sql.functions import col, expr, explode, month
from geh_stream.codelists import Colname, ChargeType


# Join charges, charge prices, charge links, metering points and market roles together. On given charge type
def join_properties_on_charges_with_given_charge_type(charges: DataFrame, charge_prices: DataFrame, charge_links: DataFrame, metering_points: DataFrame, market_roles: DataFrame, charge_type: ChargeType) -> DataFrame:
    charges_with_prices = charge_prices \
        .join(charges, [Colname.charge_key]) \
        .filter(col(Colname.charge_type) == charge_type) \
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

    if charge_type == ChargeType.subscription:
        # Explode dataframe: create row for each day the time period from and to date
        charges_with_prices = charges_with_prices.withColumn(Colname.date, explode(expr(f"sequence({Colname.from_date}, {Colname.to_date}, interval 1 day)"))) \
            .filter((month(Colname.date) == month(Colname.time))) \
            .select(
                Colname.charge_key,
                Colname.charge_id,
                Colname.charge_type,
                Colname.charge_owner,
                Colname.charge_price,
                Colname.date
            ).withColumnRenamed(Colname.date, Colname.time)

    charges_with_price_and_links_join_condition = [
        charges_with_prices[Colname.charge_key] == charge_links[Colname.charge_key],
        charges_with_prices[Colname.time] >= charge_links[Colname.from_date],
        charges_with_prices[Colname.time] < charge_links[Colname.to_date]
    ]

    charges_with_price_and_links = charges_with_prices.join(charge_links, charges_with_price_and_links_join_condition) \
        .select(
            charges_with_prices[Colname.charge_key],
            Colname.metering_point_id,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_price,
            Colname.time
        )

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

    return charges_with_metering_point_and_energy_supplier
