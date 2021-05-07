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
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Application.Utilities;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class HourlyProductionStrategy : BaseStrategy<HourlyProduction>, IDispatchStrategy
    {
        private readonly IDistributionListService _distributionListService;
        private readonly IGLNService _glnService;
        private readonly ISpecialMeteringPointsService _specialMeteringPointsService;

        public HourlyProductionStrategy(
            IDistributionListService distributionListService,
            IGLNService glnService,
            ISpecialMeteringPointsService specialMeteringPointsService,
            ILogger<HourlyProduction> logger,
            Dispatcher dispatcher)
            : base(logger, dispatcher)
        {
            _distributionListService = distributionListService;
            _glnService = glnService;
            _specialMeteringPointsService = specialMeteringPointsService;
        }

        public string FriendlyNameInstance => "hourly_production_df";

        public override IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<HourlyProduction> list, ProcessType processType, Instant timeIntervalStart, Instant timeIntervalEnd)
        {
            return (from energySupplier in list.GroupBy(hc => hc.EnergySupplierMarketParticipantmRID)
                    from gridArea in energySupplier.GroupBy(e => e.MeteringGridAreaDomainmRID)
                    let first = gridArea.First()
                    where _specialMeteringPointsService.SystemCorrectionOwner(first.MeteringGridAreaDomainmRID, timeIntervalStart) != first.EnergySupplierMarketParticipantmRID
                    select new AggregatedMeteredDataTimeSeries(CoordinatorSettings.HourlyProductionName)
                    {
                        MeteringGridAreaDomainMRid = first.MeteringGridAreaDomainmRID,
                        BalanceResponsiblePartyMarketParticipantmRID = first.BalanceResponsiblePartyMarketParticipantmRID,
                        BalanceSupplierPartyMarketParticipantmRID = first.EnergySupplierMarketParticipantmRID,
                        MarketEvaluationPointType = MarketEvaluationPointType.Production,
                        SettlementMethod = SettlementMethodType.Ignored,
                        ProcessType = Enum.GetName(typeof(ProcessType), processType),
                        Quantities = gridArea.Select(e => e.SumQuantity).ToArray(),
                        TimeIntervalStart = timeIntervalStart.ToIso8601GeneralString(),
                        TimeIntervalEnd = timeIntervalEnd.ToIso8601GeneralString(),
                        ReceiverMarketParticipantmRID = _distributionListService.GetDistributionItem(first.MeteringGridAreaDomainmRID),
                        SenderMarketParticipantmRID = _glnService.GetSenderGln(),
                    }).Cast<IOutboundMessage>()
                .ToList();
        }
    }
}
