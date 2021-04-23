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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.GLN;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Handlers
{
    public class HourlyProductionHandler : IDispatchStrategy
    {
        private readonly IGLNService _glnService;
        private readonly ISpecialMeteringPointsService _specialMeteringPointsService;

        public HourlyProductionHandler(IGLNService glnService, ISpecialMeteringPointsService specialMeteringPointsService)
        {
            _glnService = glnService;
            _specialMeteringPointsService = specialMeteringPointsService;
        }

        public string FriendlyNameInstance => "hourly_production_df";

        public async Task DispatchAsync(Stream blobStream, ProcessType pt, string startTime, string endTime, CancellationToken cancellationToken)
        {
            var ds = JsonSerializer.DeserializeAsync<IEnumerable<string>>(blobStream);

            var hp = await JsonSerializer.DeserializeAsync<HourlyProduction[]>(blobStream, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<string> results, ProcessType processType, string timeIntervalStart, string timeIntervalEnd)
        {

            return (from energySupplier in list.GroupBy(hc => hc.EnergySupplierMarketParticipantMRID)
                    from gridArea in energySupplier.GroupBy(e => e.MeteringGridAreaDomainMRID)
                    let first = gridArea.First()
                    where _specialMeteringPointsService.SystemCorrectionOwner(first.MeteringGridAreaDomainMRID) != first.EnergySupplierMarketParticipantMRID
                    select new AggregatedMeteredDataTimeSeries(CoordinatorSettings.HourlyProductionName)
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
                    }).Cast<IOutboundMessage>()
                .ToList();
        }
    }
}
