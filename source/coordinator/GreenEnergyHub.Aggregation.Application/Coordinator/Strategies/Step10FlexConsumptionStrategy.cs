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
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class Step10FlexConsumptionStrategy : BaseStrategy<AggregationResultDto>, IDispatchStrategy
    {
        private readonly GlnService _glnService;

        public Step10FlexConsumptionStrategy(ILogger<AggregationResultDto> logger, PostOfficeDispatcher messageDispatcher, IJsonSerializer jsonSerializer, GlnService glnService)
            : base(logger, messageDispatcher, jsonSerializer)
        {
            _glnService = glnService;
        }

        public string FriendlyNameInstance => "flex_consumption_with_grid_loss";

        public override IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<AggregationResultDto> aggregationResultList, string processType, Instant timeIntervalStart, Instant timeIntervalEnd)
        {
            if (aggregationResultList == null) throw new ArgumentNullException(nameof(aggregationResultList));
            var dtos = aggregationResultList;

            // Both the BRP (DDK) and the balance supplier (DDQ) shall receive the adjusted flex consumption result
            foreach (var aggregations in dtos.GroupBy(e => new { e.MeteringGridAreaDomainmRID, e.BalanceResponsiblePartyMarketParticipantmRID, e.EnergySupplierMarketParticipantmRID }))
            {
                yield return CreateConsumptionResultMessage(aggregations, processType, ProcessRole.BalanceResponsible, timeIntervalStart, timeIntervalEnd, _glnService.DataHubGln, aggregations.First().BalanceResponsiblePartyMarketParticipantmRID, SettlementMethodType.FlexSettledEbix);
                yield return CreateConsumptionResultMessage(aggregations, processType, ProcessRole.BalanceSupplier, timeIntervalStart, timeIntervalEnd, _glnService.DataHubGln, aggregations.First().EnergySupplierMarketParticipantmRID, SettlementMethodType.FlexSettledEbix);
            }
        }
    }
}
