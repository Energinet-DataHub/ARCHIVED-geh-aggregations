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

from dataclasses import dataclass
from .broker import Message


@dataclass
class ConsumptionMeteringPointCreated(Message):
    """
    Event.

    A MeteringPoint has either been added to the system,
    or an existing MeteringPoint has had its details updated.
    """
    metering_point_id: str
    metering_point_type: str
    metering_grid_area: str
    settlement_method: str
    metering_method: str
    meter_reading_periodicity: str
    net_settlement_group: str
    product: str
    effective_date: str


@dataclass
class SettlementMethodUpdated(Message):
    metering_point_id: str
    settlement_method: str
    effective_date: str
