﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Aggregation.Domain.ResultMessages;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;
using NodaTime;

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
            PostOfficeDispatcher messageDispatcher,
            IJsonSerializer jsonSerializer)
        : base(logger, messageDispatcher, jsonSerializer)
        {
            _distributionListService = distributionListService;
            _glnService = glnService;
            _specialMeteringPointsService = specialMeteringPointsService;
        }

        public string FriendlyNameInstance => "flex_consumption_with_grid_loss";

        public override IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<ConsumptionDto> aggregationResultList, string processType, Instant timeIntervalStart, Instant timeIntervalEnd)
        {
            if (aggregationResultList == null) throw new ArgumentNullException(nameof(aggregationResultList));

            foreach (var aggregation in aggregationResultList)
            {
                // Both the BRP (DDK) and the balance supplier (DDQ) shall receive the adjusted flex consumption result
                yield return CreateMessage(aggregation, processType, timeIntervalStart, timeIntervalEnd, aggregation.BalanceResponsiblePartyMarketParticipantmRID);
                yield return CreateMessage(aggregation, processType, timeIntervalStart, timeIntervalEnd, aggregation.EnergySupplierMarketParticipantmRID);
            }
        }

        private AggregationResultMessage CreateMessage(ConsumptionDto consumptionDto, string processType, Instant timeIntervalStart, Instant timeIntervalEnd, string recipient)
        {
            if (consumptionDto == null) throw new ArgumentNullException(nameof(consumptionDto));

            return new AggregationResultMessage
            {
                ProcessType = processType,
                TimeIntervalStart = timeIntervalStart,
                TimeIntervalEnd = timeIntervalEnd,
                MeteringGridAreaDomainmRID = consumptionDto.MeteringGridAreaDomainmRID,
                BalanceResponsiblePartyMarketParticipantmRID = consumptionDto.BalanceResponsiblePartyMarketParticipantmRID,
                BalanceSupplierPartyMarketParticipantmRID = consumptionDto.EnergySupplierMarketParticipantmRID,
                MarketEvaluationPointType = MarketEvaluationPointType.Production,
                EnergyQuantity = consumptionDto.SumQuantity,
                QuantityQuality = consumptionDto.AggregatedQuality,
                SenderMarketParticipantmRID = _glnService.GetSenderGln(),
                ReceiverMarketParticipantmRID = recipient,
            };
        }
    }
}
