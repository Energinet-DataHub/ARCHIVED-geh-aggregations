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
    df = charges \
        .filter(col(Colname.resolution) == resolution_duration) \
        .selectExpr(
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.resolution,
            Colname.charge_tax,
            Colname.currency,
            f"{Colname.from_date} as {charge_from_date}",
            f"{Colname.to_date} as {charge_to_date}"
        )

    charge_prices = charge_prices \
        .select(
            Colname.charge_id,
            Colname.charge_price,
            Colname.time
        )

    charge_links = charge_links \
        .selectExpr(
            Colname.charge_id,
            Colname.metering_point_id,
            f"{Colname.from_date} as {charge_link_from_date}",
            f"{Colname.to_date} as {charge_link_to_date}"
        )

    market_roles = market_roles \
        .selectExpr(
            Colname.energy_supplier_id,
            Colname.metering_point_id,
            f"{Colname.from_date} as {market_roles_from_date}",
            f"{Colname.to_date} as {market_roles_to_date}"
        )

    metering_points = metering_points \
        .selectExpr(
            Colname.metering_point_id,
            Colname.grid_area,
            f"{Colname.from_date} as {metering_point_from_date}",
            f"{Colname.to_date} as {metering_point_to_date}"
        )

    df = df \
        .join(charge_prices, [Colname.charge_id], "left") \
        .join(charge_links, [Colname.charge_id], "left")

    # df.show(100, False)
    # market_roles.show(100, False)

    df = df.join(
        market_roles,
        [
            df[Colname.metering_point_id] == market_roles[Colname.metering_point_id],
            df[Colname.time].cast(LongType()) >= market_roles[market_roles_from_date].cast(LongType()),
            df[Colname.time].cast(LongType()) < market_roles[market_roles_to_date].cast(LongType())
        ]) \
        .drop(market_roles[Colname.metering_point_id])
    # .drop(metering_points[Colname.metering_point_id])

    #     & (df[Colname.time].cast(LongType()) >= market_roles[market_roles_from_date].cast(LongType())) \
    #     & (df[Colname.time].cast(LongType()) < market_roles[market_roles_to_date].cast(LongType()))
    # ) \
    # .join(metering_points, 
    #     (df[Colname.metering_point_id] == metering_points[Colname.metering_point_id]) \
    #     & (df[Colname.time].cast(LongType()) >= metering_points[metering_point_from_date].cast(LongType())) \
    #     & (df[Colname.time].cast(LongType()) < metering_points[metering_point_to_date].cast(LongType()))

    # df.show(100, False)

    return df
