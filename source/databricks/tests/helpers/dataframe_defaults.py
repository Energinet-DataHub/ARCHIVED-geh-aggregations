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
from decimal import Decimal
from datetime import datetime
from geh_stream.codelists import Colname
from geh_stream.schemas import charges_schema, charge_links_schema, charge_prices_schema, metering_point_schema, market_roles_schema
from geh_stream.codelists import ResolutionDuration
import pytest
import pandas as pd


class DataframeDefaults():
    default_charge_id: str = "chargea"
    default_charge_type: str = "D01"
    default_charge_owner: str = "001"
    default_charge_key: str = f"{default_charge_id}-{default_charge_type}-{default_charge_owner}"
    default_resolution: str = ResolutionDuration.day
    default_charge_tax: str = "true"
    default_currency: str = "DDK"
    default_metering_point_id: str = "D01"
    default_charge_price: Decimal = Decimal(1.123456)