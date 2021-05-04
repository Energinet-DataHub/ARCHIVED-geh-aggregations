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
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class HourlyConsumptionStrategy : BaseStrategy<HourlyConsumption>, IDispatchStrategy
    {
        private readonly IDistributionListService _distributionListService;
        private readonly IGLNService _glnService;

        public HourlyConsumptionStrategy(
            IDistributionListService distributionListService,
            IGLNService glnService,
            ILogger<HourlyConsumption> logger,
            Dispatcher dispatcher)
            : base(logger, dispatcher)
        {
            _distributionListService = distributionListService;
            _glnService = glnService;
        }

        public string FriendlyNameInstance => "hourly_consumption_df";

        public override IEnumerable<IOutboundMessage> PrepareMessages(
            IEnumerable<HourlyConsumption> list,
            ProcessType processType,
            string timeIntervalStart,
            string timeIntervalEnd)
        {
            return (from energySupplier in list.GroupBy(hc => hc.EnergySupplierMarketParticipantmRID)
                    from gridArea in energySupplier.GroupBy(e => e.MeteringGridAreaDomainmRID)
                    let first = gridArea.First()
                    select new AggregatedMeteredDataTimeSeries(CoordinatorSettings.HourlyConsumptionName)
                    {
                        MeteringGridAreaDomainMRid = first.MeteringGridAreaDomainmRID,
                        BalanceResponsiblePartyMarketParticipantMRid = first.BalanceResponsiblePartyMarketParticipantmRID,
                        BalanceSupplierPartyMarketParticipantMRid = first.EnergySupplierMarketParticipantmRID,
                        MarketEvaluationPointType = MarketEvaluationPointType.Consumption,
                        SettlementMethod = SettlementMethodType.NonProfiled,
                        ProcessType = Enum.GetName(typeof(ProcessType), processType),
                        Quantities = gridArea.Select(e => e.SumQuantity).ToArray(),
                        TimeIntervalStart = timeIntervalStart,
                        TimeIntervalEnd = timeIntervalEnd,
                        ReceiverMarketParticipantMRid = _distributionListService.GetDistributionItem(first.MeteringGridAreaDomainmRID),
                        SenderMarketParticipantMRid = _glnService.GetSenderGln(),
                    }).Cast<IOutboundMessage>()
                .ToList();
        }
    }
}
