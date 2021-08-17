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

from source.databricks.geh_stream.codelists import colname
from pyspark.sql.dataframe import DataFrame
from pyspark.sql.functions import col
from pyspark.sql.types import LongType
from geh_stream.codelists import Colname, ResolutionDuration


charge_from_date = "charge_from_date"
charge_to_date = "charge_to_date"
charge_link_from_date = "charge_link_from_date"
charge_link_to_date = "charge_link_to_date"
market_roles_from_date = "market_roles_from_date"
market_roles_to_date = "market_roles_to_date"
metering_point_from_date = "metering_point_from_date"
metering_point_to_date = "metering_point_to_date"


def get_charges(charges: DataFrame, charge_links: DataFrame, charge_prices: DataFrame, metering_points: DataFrame, market_roles: DataFrame, resolution_duration: ResolutionDuration) -> DataFrame:
    df = charges.filter(col(Colname.resolution) == resolution_duration)

    df = df \
        .join(charge_prices, [Colname.charge_key], "inner") \
        .select(
            df[Colname.charge_key],
            df[Colname.charge_type],
            df[Colname.charge_owner],
            df[Colname.charge_tax],
            df[Colname.resolution],
            charge_prices[Colname.time],
            charge_prices[Colname.charge_price]
        )

    df = df \
        .join(charge_links, [Colname.charge_key], "inner") \
        .select(
            df[Colname.charge_key],
            df[Colname.charge_type],
            df[Colname.charge_owner],
            df[Colname.charge_tax],
            df[Colname.resolution],
            df[Colname.time],
            df[Colname.charge_price],
            charge_links[Colname.metering_point_id]
        )

    # df.show(100, False)
    # market_roles.show(100, False)

    df = df.join(
        market_roles,
        [
            df[Colname.metering_point_id] == market_roles[Colname.metering_point_id],
            df[Colname.time] >= market_roles[market_roles_from_date],
            df[Colname.time] < market_roles[market_roles_to_date]
        ]) \
        .select(
            df[Colname.charge_key],
            df[Colname.charge_type],
            df[Colname.charge_owner],
            df[Colname.charge_tax],
            df[Colname.resolution],
            df[Colname.time],
            df[Colname.charge_price],
            df[Colname.metering_point_id],
            market_roles[Colname.energy_supplier_id]
        )

    df = df.join(
        metering_points,
        [
            df[Colname.metering_point_id] == metering_points[Colname.metering_point_id],
            df[Colname.time] >= metering_points[metering_point_from_date],
            df[Colname.time] < metering_points[metering_point_to_date]
        ]) \
        .select(
            df[Colname.charge_key],
            df[Colname.charge_type],
            df[Colname.charge_owner],
            df[Colname.charge_tax],
            df[Colname.resolution],
            df[Colname.time],
            df[Colname.charge_price],
            df[Colname.metering_point_id],
            df[Colname.energy_supplier_id],
            metering_points[Colname.metering_point_type],
            metering_points[Colname.connection_state],
            metering_points[Colname.settlement_method],
            metering_points[Colname.grid_area],

        )
    # df.show(100, False)

    return df
