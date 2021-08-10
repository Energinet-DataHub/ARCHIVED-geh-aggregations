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
from geh_stream.codelists import Colname, ResolutionDuration


charge_from_date = "charge_from_date"
charge_to_date = "charge_to_date"
charge_link_from_date = "charge_link_from_date"
charge_link_to_date = "charge_link_to_date"


def get_hourly_charges(charges: DataFrame, charge_links: DataFrame, charge_prices: DataFrame) -> DataFrame:
    hourly_charges = charges \
        .filter(col(Colname.resolution) == ResolutionDuration.day) \
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

    hourly_charges = hourly_charges \
        .join(charge_prices, Colname.charge_id, "left") \
        .join(charge_links, Colname.charge_id, "left")

    return hourly_charges
