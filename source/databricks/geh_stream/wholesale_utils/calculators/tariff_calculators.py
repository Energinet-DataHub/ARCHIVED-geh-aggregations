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
from pyspark.sql.functions import col
from geh_stream.codelists import Colname, ChargeType


def calculate_tariff_price_per_ga_co_es(tariff_df: DataFrame):
    # .groupBy(Colname.grid_area, Colname.charge_owner, Colname.energy_supplier_id) \
    # .sum(Colname.charge_price)
    tariffs = tariff_df.filter(col(Colname.charge_type) == ChargeType.tariff)
    tariffs.show()
    return tariffs
