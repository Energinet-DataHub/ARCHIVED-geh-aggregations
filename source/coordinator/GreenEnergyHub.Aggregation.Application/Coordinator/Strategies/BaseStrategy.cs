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
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.ResultMessages;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public abstract class BaseStrategy<T>
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IGLNService _glnService;
        private readonly MessageDispatcher _messageDispatcher;

        protected BaseStrategy(ILogger<T> logger, MessageDispatcher messageDispatcher, IJsonSerializer jsonSerializer, IGLNService glnService)
        {
            Logger = logger;
            _messageDispatcher = messageDispatcher;
            _jsonSerializer = jsonSerializer;
            _glnService = glnService;
        }

        private ILogger<T> Logger { get; }

        public virtual async Task DispatchAsync(Stream blobStream, string processType, Instant startTime, Instant endTime, CancellationToken cancellationToken)
        {
            var listOfResults = await _jsonSerializer.DeserializeAsync<IEnumerable<T>>(blobStream, cancellationToken).ConfigureAwait(false);

            var messages = PrepareMessages(listOfResults.ToList(), processType, startTime, endTime);

            if (messages != null)
            {
                await ForwardMessagesOutAsync(messages, cancellationToken).ConfigureAwait(false);
            }
        }

        public abstract IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<T> aggregationResultList, string processType, Instant timeIntervalStart, Instant timeIntervalEnd);

        protected ConsumptionResultMessage CreateConsumptionResultMessage(IEnumerable<AggregationResultDto> consumptionDtos, string processType, Instant timeIntervalStart, Instant timeIntervalEnd, string recipient, string settlementMethod)
        {
            var aggregationList = consumptionDtos.ToList();
            var resultMsg = CreateMessage(aggregationList, processType, timeIntervalStart, timeIntervalEnd, recipient, MarketEvaluationPointType.Consumption);
            return new ConsumptionResultMessage(resultMsg) { SettlementMethod = settlementMethod };
        }

        protected AggregatedExchangeNeighbourResultMessage CreateExchangeNeighbourMessage(IEnumerable<AggregationResultDto> exchangeDtos, string processType, Instant timeIntervalStart, Instant timeIntervalEnd, string recipient)
        {
            var aggregationList = exchangeDtos.ToList();
            var resultMsg = CreateMessage(aggregationList, processType, timeIntervalStart, timeIntervalEnd, recipient, MarketEvaluationPointType.Exchange);
            return new AggregatedExchangeNeighbourResultMessage(resultMsg);
        }

        protected AggregationResultMessage CreateMessage(IEnumerable<AggregationResultDto> productionDtos, string processType, Instant timeIntervalStart, Instant timeIntervalEnd, string recipient, string marketEvaluationPointType)
        {
            if (productionDtos == null) throw new ArgumentNullException(nameof(productionDtos));

            var dtoList = productionDtos.ToList();
            var dto = dtoList.First();

            return new AggregationResultMessage(
                processType,
                timeIntervalStart,
                timeIntervalEnd,
                dto.MeteringGridAreaDomainmRID,
                dto.BalanceResponsiblePartyMarketParticipantmRID,
                dto.EnergySupplierMarketParticipantmRID,
                marketEvaluationPointType,
                dtoList.Select(e => new EnergyObservation { EnergyQuantity = e.SumQuantity, QuantityQuality = e.AggregatedQuality }),
                _glnService.GetSenderGln(),
                recipient);
        }

        private async Task ForwardMessagesOutAsync(IEnumerable<IOutboundMessage> preparedMessages, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var preparedMessage in preparedMessages)
                {
                    await _messageDispatcher.DispatchAsync(preparedMessage, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Could not dispatch message due to {error}", new { error = e.Message });
                throw;
            }
        }
    }
}
