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
from geh_stream.schemas import time_series_schema, metering_point_schema, grid_loss_sys_corr_schema, market_roles_schema, charges_schema, charge_links_schema, charge_prices_schema, es_brp_relations_schema
import dateutil.parser


def get_hourly_charges(charges: DataFrame, charge_links: DataFrame, charge_prices: DataFrame) -> DataFrame:
    hourly_charges = charges \
        .filter(col("resolution") == "PT1H") \
        .withColumnRenamed("from_date", "charge_from_date") \
        .withColumnRenamed("to_date", "charge_to_date") \
        .select(
            "charge_id",
            "charge_type",
            "charge_owner",
            "resolution",
            "charge_tax",
            "currency",
            "charge_from_date",
            "charge_to_date"
        )

    charge_prices = charge_prices \
        .select(
            "charge_id",
            "charge_price",
            "time"
        )

    charge_links = charge_links \
        .withColumnRenamed("from_date", "charge_link_from_date") \
        .withColumnRenamed("to_date", "charge_link_to_date") \
        .select(
            "charge_id",
            "metering_point_id",
            "charge_link_from_date",
            "charge_link_to_date"
        )

    hourly_charges = hourly_charges \
        .join(charge_prices, "charge_id", "left") \
        .join(charge_links, "charge_id", "")
