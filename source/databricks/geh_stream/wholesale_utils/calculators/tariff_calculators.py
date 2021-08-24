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
from pyspark.sql.functions import col, sum, count
from geh_stream.codelists import Colname, ChargeType


total_quantity = "total_quantity"
charge_count = "charge_count"


def calculate_tariff_price_per_ga_co_es(tariffs: DataFrame) -> DataFrame:
    tariffs = tariffs.filter(col(Colname.charge_type) == ChargeType.tariff)
    agg_df = tariffs \
        .groupBy(
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.time,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.charge_key
        ) \
        .agg(
             sum(Colname.quantity).alias(total_quantity),
             count(Colname.metering_point_id).alias(charge_count)
        ).select("*").distinct()

    df = tariffs.select(
            tariffs[Colname.charge_key],
            tariffs[Colname.charge_id],
            tariffs[Colname.charge_type],
            tariffs[Colname.charge_owner],
            tariffs[Colname.charge_tax],
            tariffs[Colname.resolution],
            tariffs[Colname.time],
            tariffs[Colname.charge_price],
            tariffs[Colname.energy_supplier_id],
            tariffs[Colname.metering_point_type],
            tariffs[Colname.settlement_method],
            tariffs[Colname.grid_area]
         ).distinct()

    df = df.join(agg_df, [
        Colname.energy_supplier_id,
        Colname.grid_area,
        Colname.time,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.charge_key
    ]) \
    .withColumn("total_amount", col(Colname.charge_price) * col(total_quantity)) \
    .orderBy([Colname.charge_key, Colname.grid_area, Colname.energy_supplier_id, Colname.time, Colname.metering_point_type, Colname.settlement_method])

    return df
