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
from geh_stream.bus import MessageDispatcher, messages as m


def on_consumption_metering_point_created(msg: m.ConsumptionMeteringPointCreated):
    print("create event with id " + msg.id)


def on_settlement_method_updated(msg: m.SettlementMethodUpdated):
    # update meteringpoint
    print("update smethod" + msg.settlement_method + " on id " + msg.metering_point_id)


# -- Dispatcher --------------------------------------------------------------
dispatcher = MessageDispatcher({
    m.ConsumptionMeteringPointCreated: on_consumption_metering_point_created,
    m.SettlementMethodUpdated: on_settlement_method_updated,
})
