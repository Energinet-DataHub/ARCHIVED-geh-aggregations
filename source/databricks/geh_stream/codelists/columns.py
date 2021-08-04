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

from typing import Final

class Colname(object):
    added_grid_loss: Final = "added_grid_loss"
    added_system_correction: Final = "added_system_correction"
    aggregated_quality: Final = "aggregated_quality"
    balance_responsible_id: Final = "BalanceResponsibleParty_MarketParticipant_mRID"
    connection_state: Final = "ConnectionState"
    end: Final = "end"
    energy_supplier_id: Final = "EnergySupplier_MarketParticipant_mRID"
    from_date: Final = "ValidFrom"
    grid_area: Final = "MeteringGridArea_Domain_mRID"
    grid_loss: Final = "grid_loss"
    in_grid_area: Final = "InMeteringGridArea_Domain_mRID"
    is_grid_loss: Final = "IsGridLoss"
    is_system_correction: Final = "IsSystemCorrection"
    metering_method: Final = "MeteringMethod"
    metering_point_id: Final = "MarketEvaluationPoint_mRID"
    metering_point_type: Final = "MarketEvaluationPointType"
    out_grid_area: Final = "OutMeteringGridArea_Domain_mRID"
    product: Final = "Product"
    quality: Final = "Quality"
    quantity: Final = "Quantity"
    resolution: Final = "MeterReadingPeriodicity"
    settlement_method: Final = "SettlementMethod"
    start: Final = "start"
    sum_quantity: Final = "sum_quantity"
    time: Final = "Time"
    time_window: Final = "time_window"
    time_window_end: Final = "time_window.end"
    time_window_start: Final = "time_window.start"
    to_date: Final = "ValidTo"
