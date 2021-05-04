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
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class ExchangeNeighbourStrategy : BaseStrategy<ExchangeNeighbourDto>, IDispatchStrategy
    {
        private readonly IGLNService _glnService;

        public ExchangeNeighbourStrategy(ILogger<ExchangeNeighbourDto> logger, IGLNService glnService, Dispatcher dispatcher)
            : base(logger, dispatcher)
        {
            _glnService = glnService;
        }

        public string FriendlyNameInstance => "net_exchange_per_neighbour_df";

        public override IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<ExchangeNeighbourDto> aggregationResultList, ProcessType processType, string timeIntervalStart, string timeIntervalEnd)
        {
            if (aggregationResultList == null) throw new ArgumentNullException(nameof(aggregationResultList));

            foreach (var exchangeDto in aggregationResultList)
            {
                yield return new AggregatedExchangeNeighbourResultMessage()
                {
                    MeteringGridAreaDomainMRID = exchangeDto.MeteringGridAreaDomainMRID,
                    InMeteringGridAreaDomainMRID = exchangeDto.InMeteringGridAreaDomainMRID,
                    OutMeteringGridAreaDomainMRID = exchangeDto.OutMeteringGridAreaDomainMRID,
                    Result = exchangeDto.Result,
                    MarketEvaluationPointType = MarketEvaluationPointType.Exchange,
                    AggregationType = CoordinatorSettings.ExchangeNeighbourName,
                    ProcessType = Enum.GetName(typeof(ProcessType), processType),
                    TimeIntervalStart = timeIntervalStart,
                    TimeIntervalEnd = timeIntervalEnd,
                    ReceiverMarketParticipantMRID = _glnService.GetEsettGln(),
                    SenderMarketParticipantMRID = _glnService.GetSenderGln(),
                    Transaction = new Transaction(),
                };
            }
        }
    }
}
