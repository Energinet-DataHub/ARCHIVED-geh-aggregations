// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GreenEnergyHub.Aggregation.Application.Coordinator.HourlyConsumption;
using GreenEnergyHub.Aggregation.Application.GLN;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Messaging.Transport;
using NodaTime.Text;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Handlers
{
    public class AdjustedProductionHandler : IAggregationHandler
    {
        private readonly IGLNService _glnService;
        private readonly ISpecialMeteringPointsService _specialMeteringPointsService;

        public AdjustedProductionHandler(IGLNService glnService, ISpecialMeteringPointsService specialMeteringPointsService)
        {
            _glnService = glnService;
            _specialMeteringPointsService = specialMeteringPointsService;
        }

        public IEnumerable<IOutboundMessage> PrepareMessages(List<string> result, ProcessType processType, string timeIntervalStart, string timeIntervalEnd)
        {
            var list = new List<AdjustedHourlyProduction>();
            foreach (var json in result)
            {
                var obj = JsonSerializer.Deserialize<AdjustedHourlyProduction>(json);
                list.Add(obj);
            }

            var messages = new List<IOutboundMessage>();
            foreach (var energySupplier in list.GroupBy(hc => hc.EnergySupplierMarketParticipantMRID))
            {
                foreach (var gridArea in energySupplier.GroupBy(e => e.MeteringGridAreaDomainMRID))
                {
                    var first = gridArea.First();
                    if (_specialMeteringPointsService.SystemCorrectionOwner(first.MeteringGridAreaDomainMRID, OffsetDateTimePattern.CreateWithInvariantCulture("G").Parse(timeIntervalStart).Value.ToInstant()) !=
                        first.EnergySupplierMarketParticipantMRID)
                    {
                        // If we are the owner of the system correction metering point we not receive this message
                        continue;
                    }

                    var amdts = new AggregatedMeteredDataTimeSeries(CoordinatorSettings.AdjustedHourlyProductionName)
                    {
                        MeteringGridAreaDomainMRid = first.MeteringGridAreaDomainMRID,
                        BalanceResponsiblePartyMarketParticipantMRid = first.BalanceResponsiblePartyMarketParticipantMRID,
                        BalanceSupplierPartyMarketParticipantMRid = first.EnergySupplierMarketParticipantMRID,
                        MarketEvaluationPointType = MarketEvaluationPointType.Production,
                        SettlementMethod = SettlementMethodType.Ignored,
                        ProcessType = Enum.GetName(typeof(ProcessType), processType),
                        Quantities = gridArea.Select(e => e.SumQuantity).ToArray(),
                        TimeIntervalStart = timeIntervalStart,
                        TimeIntervalEnd = timeIntervalEnd,
                        ReceiverMarketParticipantMRid = _glnService.GetGlnFromSupplierId(first.EnergySupplierMarketParticipantMRID),
                        SenderMarketParticipantMRid = _glnService.GetSenderGln(),
                    };

                    messages.Add(amdts);
                }
            }

            return messages;
        }
    }
}
