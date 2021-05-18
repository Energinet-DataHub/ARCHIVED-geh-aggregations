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
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.ResultMessages;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class AdjustedProductionStrategy : BaseStrategy<ProductionDto>, IDispatchStrategy
    {
        private readonly IDistributionListService _distributionListService;
        private readonly IGLNService _glnService;

        public AdjustedProductionStrategy(
            IDistributionListService distributionListService,
            IGLNService glnService,
            ILogger<ProductionDto> logger,
            PostOfficeDispatcher messageDispatcher,
            IJsonSerializer jsonSerializer)
            : base(logger, messageDispatcher, jsonSerializer)
        {
            _distributionListService = distributionListService;
            _glnService = glnService;
        }

        public string FriendlyNameInstance => "hourly_production_with_system_correction_and_grid_loss";

        public override IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<ProductionDto> aggregationResultList, string processType, Instant timeIntervalStart, Instant timeIntervalEnd)
        {
            if (aggregationResultList == null) throw new ArgumentNullException(nameof(aggregationResultList));

            foreach (var aggregation in aggregationResultList)
            {
                // Both the BRP (DDK) and the balance supplier (DDQ) shall receive the adjusted hourly production result
                yield return CreateMessage(aggregation, processType, timeIntervalStart, timeIntervalEnd, aggregation.BalanceResponsiblePartyMarketParticipantmRID);
                yield return CreateMessage(aggregation, processType, timeIntervalStart, timeIntervalEnd, aggregation.EnergySupplierMarketParticipantmRID);
            }
        }

        private AggregationResultMessage CreateMessage(ProductionDto productionDto, string processType, Instant timeIntervalStart, Instant timeIntervalEnd, string recipient)
        {
            if (productionDto == null) throw new ArgumentNullException(nameof(productionDto));

            return new AggregationResultMessage
            {
                ProcessType = processType,
                TimeIntervalStart = timeIntervalStart,
                TimeIntervalEnd = timeIntervalEnd,
                MeteringGridAreaDomainmRID = productionDto.MeteringGridAreaDomainmRID,
                BalanceResponsiblePartyMarketParticipantmRID = productionDto.BalanceResponsiblePartyMarketParticipantmRID,
                BalanceSupplierPartyMarketParticipantmRID = productionDto.EnergySupplierMarketParticipantmRID,
                MarketEvaluationPointType = MarketEvaluationPointType.Production,
                EnergyQuantity = productionDto.SumQuantity,
                QuantityQuality = productionDto.AggregatedQuality,
                SenderMarketParticipantmRID = _glnService.GetSenderGln(),
                ReceiverMarketParticipantmRID = recipient,
            };
        }
    }
}
