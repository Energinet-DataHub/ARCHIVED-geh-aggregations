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

import json


class AggregationResults:

    def __init__(self, net_exchange_per_neighbour, hourlyConsumption, hourlyProduction, flexConsumption, flex_consumption_with_system_correction_and_grid_loss, hourly_production_with_system_correction_and_grid_loss, added_system_correction, added_grid_loss):
        self.netExchangePerNeighbour = net_exchange_per_neighbour
        self.HourlyConsumption = hourlyConsumption
        self.HourlyProduction = hourlyProduction
        self.FlexConsumption = flexConsumption
        self.AdjustedFlexConsumption = flex_consumption_with_system_correction_and_grid_loss
        self.AdjustedHourlyProduction = hourly_production_with_system_correction_and_grid_loss
        self.AddedSystemCorrection = added_system_correction
        self.AddedGridLoss = added_grid_loss

    def toJSON(self):
        return json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=4)
