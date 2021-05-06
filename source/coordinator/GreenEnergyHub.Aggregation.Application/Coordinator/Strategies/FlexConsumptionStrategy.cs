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
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class FlexConsumptionStrategy : BaseStrategy<FlexConsumption>, IDispatchStrategy
    {
        private readonly IGLNService _glnService;
        private readonly ISpecialMeteringPointsService _specialMeteringPointsService;

        public FlexConsumptionStrategy(
            IGLNService glnService,
            ISpecialMeteringPointsService specialMeteringPointsService,
            ILogger<FlexConsumption> logger,
            Dispatcher dispatcher)
            : base(logger, dispatcher)
        {
            _glnService = glnService;
            _specialMeteringPointsService = specialMeteringPointsService;
        }

        public string FriendlyNameInstance => "flex_consumption_df";

        public override IEnumerable<IOutboundMessage> PrepareMessages(
            IEnumerable<FlexConsumption> list,
            ProcessType processType,
            Instant timeIntervalStart,
            Instant timeIntervalEnd)
        {
            return (from energySupplier in list.GroupBy(hc => hc.EnergySupplierMarketParticipantMRID)
                    from gridArea in energySupplier.GroupBy(e => e.MeteringGridAreaDomainMRID)
                    let first = gridArea.First()
                    where _specialMeteringPointsService.GridLossOwner(first.MeteringGridAreaDomainMRID, timeIntervalStart) != first.EnergySupplierMarketParticipantMRID
                    select new AggregatedMeteredDataTimeSeries(CoordinatorSettings.FlexConsumptionName)
                    {
                        MeteringGridAreaDomainMRid = first.MeteringGridAreaDomainMRID,
                        BalanceResponsiblePartyMarketParticipantMRid = first.BalanceResponsiblePartyMarketParticipantMRID,
                        BalanceSupplierPartyMarketParticipantMRid = first.EnergySupplierMarketParticipantMRID,
                        MarketEvaluationPointType = MarketEvaluationPointType.Consumption,
                        SettlementMethod = SettlementMethodType.FlexSettled,
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
