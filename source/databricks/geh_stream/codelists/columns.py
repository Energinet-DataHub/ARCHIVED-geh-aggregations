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


class Colname(object):
    added_grid_loss = "added_grid_loss"
    added_system_correction = "added_system_correction"
    aggregated_quality = "aggregated_quality"
    balance_responsible_id = "BalanceResponsibleParty_MarketParticipant_mRID"
    connection_state = "ConnectionState"
    end = "end"
    energy_supplier_id = "EnergySupplier_MarketParticipant_mRID"
    from_date = "ValidFrom"
    grid_area = "MeteringGridArea_Domain_mRID"
    grid_loss = "grid_loss"
    in_grid_area = "InMeteringGridArea_Domain_mRID"
    is_grid_loss = "IsGridLoss"
    is_system_correction = "IsSystemCorrection"
    metering_method = "MeteringMethod"
    metering_point_id = "MarketEvaluationPoint_mRID"
    metering_point_type = "MarketEvaluationPointType"
    out_grid_area = "OutMeteringGridArea_Domain_mRID"
    product = "Product"
    quality = "Quality"
    quantity = "Quantity"
    resolution = "MeterReadingPeriodicity"
    settlement_method = "SettlementMethod"
    start = "start"
    sum_quantity = "sum_quantity"
    time = "Time"
    time_window = "time_window"
    time_window_end = "time_window.end"
    time_window_start = "time_window.start"
    to_date = "ValidTo"
