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
using GreenEnergyHub.Aggregation.Domain.ResultMessages;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;
using NodaTime.Text;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class AdjustedFlexConsumptionStrategy : BaseStrategy<ConsumptionDto>, IDispatchStrategy
    {
        private readonly IDistributionListService _distributionListService;
        private readonly IGLNService _glnService;
        private readonly ISpecialMeteringPointsService _specialMeteringPointsService;

        public AdjustedFlexConsumptionStrategy(
            IDistributionListService distributionListService,
            IGLNService glnService,
            ISpecialMeteringPointsService specialMeteringPointsService,
            ILogger<ConsumptionDto> logger,
            Dispatcher dispatcher)
        : base(logger, dispatcher)
        {
            _distributionListService = distributionListService;
            _glnService = glnService;
            _specialMeteringPointsService = specialMeteringPointsService;
        }

        public string FriendlyNameInstance => "flex_consumption_with_grid_loss";

        public override IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<ConsumptionDto> aggregationResultList, ProcessType processType, string timeIntervalStart, string timeIntervalEnd)
        {
            var validTime = InstantPattern.ExtendedIso.Parse(timeIntervalStart).GetValueOrThrow();

            return (from energySupplier in aggregationResultList.GroupBy(hc => hc.EnergySupplierMarketParticipantmRID)
                    from gridArea in energySupplier.GroupBy(e => e.MeteringGridAreaDomainmRID)
                    let first = gridArea.First()
                    where _specialMeteringPointsService.GridLossOwner(first.MeteringGridAreaDomainmRID, validTime) == first.EnergySupplierMarketParticipantmRID
                    select new AggregatedConsumptionResultMessage()
                    {
                        MeteringGridAreaDomainmRID = first.MeteringGridAreaDomainmRID,
                        BalanceResponsiblePartyMarketParticipantmRID = first.BalanceResponsiblePartyMarketParticipantmRID,
                        BalanceSupplierPartyMarketParticipantmRID = first.EnergySupplierMarketParticipantmRID,
                        MarketEvaluationPointType = MarketEvaluationPointType.Consumption,
                        AggregationType = CoordinatorSettings.AdjustedFlexConsumptionName,
                        SettlementMethod = SettlementMethodType.FlexSettled,
                        ProcessType = Enum.GetName(typeof(ProcessType), processType),
                        Quantities = gridArea.Select(e => e.SumQuantity),
                        TimeIntervalStart = timeIntervalStart,
                        TimeIntervalEnd = timeIntervalEnd,
                        ReceiverMarketParticipantmRID = _distributionListService.GetDistributionItem(first.MeteringGridAreaDomainmRID),
                        SenderMarketParticipantmRID = _glnService.GetSenderGln(),
                    }).Cast<IOutboundMessage>()
                .ToList();
        }
    }
}
