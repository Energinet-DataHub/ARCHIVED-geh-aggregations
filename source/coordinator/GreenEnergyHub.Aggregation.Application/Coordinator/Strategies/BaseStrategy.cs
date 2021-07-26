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
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.ResultMessages;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public abstract class BaseStrategy<T, TU>
    where TU : IOutboundMessage
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly MessageDispatcher _messageDispatcher;

        protected BaseStrategy(ILogger<T> logger, MessageDispatcher messageDispatcher, IJsonSerializer jsonSerializer)
        {
            Logger = logger;
            _messageDispatcher = messageDispatcher;
            _jsonSerializer = jsonSerializer;
        }

        private ILogger<T> Logger { get; }

        public virtual async Task DispatchAsync(Stream blobStream, string processType, Instant startTime, Instant endTime, string type, CancellationToken cancellationToken)
        {
            var listOfResults = new List<T>();
            using (var sr = new StreamReader(blobStream))
            {
                while (sr.Peek() >= 0)
                {
                    var line = await sr.ReadLineAsync().ConfigureAwait(false);
                    listOfResults.Add(_jsonSerializer.Deserialize<T>(line));
                }
            }

            var messages = PrepareMessages(listOfResults, processType, startTime, endTime);

            if (messages != null)
            {
                await ForwardMessagesOutAsync(messages, type, cancellationToken).ConfigureAwait(false);
            }
        }

        public virtual void CheckArguments(IEnumerable<AggregationResultDto> aggregationResultList)
        {
            if (aggregationResultList == null)
            {
                throw new ArgumentNullException(nameof(aggregationResultList));
            }
        }

        public abstract IEnumerable<TU> PrepareMessages(
            IEnumerable<T> aggregationResultList,
            string processType,
            Instant timeIntervalStart,
            Instant timeIntervalEnd);

        protected ConsumptionResultMessage CreateConsumptionResultMessage(IEnumerable<AggregationResultDto> consumptionDtos, string processType, string processRole, Instant timeIntervalStart, Instant timeIntervalEnd, string sender, string receiver, string settlementMethod)
        {
            var aggregationList = consumptionDtos.ToList();
            var resultMsg = CreateMessage(aggregationList, processType, processRole, timeIntervalStart, timeIntervalEnd, sender, receiver, MarketEvaluationPointType.Consumption);
            return new ConsumptionResultMessage(resultMsg) { SettlementMethod = settlementMethod };
        }

        protected AggregatedExchangeNeighbourResultMessage CreateExchangeNeighbourMessage(IEnumerable<AggregationResultDto> exchangeDtos, string processType, Instant timeIntervalStart, Instant timeIntervalEnd, string sender, string receiver)
        {
            var aggregationList = exchangeDtos.ToList();
            var resultMsg = CreateMessage(aggregationList, processType, ProcessRole.Esett, timeIntervalStart, timeIntervalEnd, sender, receiver, MarketEvaluationPointType.Exchange);
            return new AggregatedExchangeNeighbourResultMessage(resultMsg);
        }

        protected AggregationResultMessage CreateMessage(IEnumerable<AggregationResultDto> productionDtos, string processType, string processRole, Instant timeIntervalStart, Instant timeIntervalEnd, string sender, string receiver, string marketEvaluationPointType)
        {
            if (productionDtos == null)
            {
                throw new ArgumentNullException(nameof(productionDtos));
            }

            var dtoList = productionDtos.ToList();
            var dto = dtoList.First();

            return new AggregationResultMessage(
                processType,
                processRole,
                timeIntervalStart,
                timeIntervalEnd,
                dto.MeteringGridAreaDomainmRID,
                dto.BalanceResponsiblePartyMarketParticipantmRID,
                dto.EnergySupplierMarketParticipantmRID,
                marketEvaluationPointType,
                dtoList.Select(e => new EnergyObservation { EnergyQuantity = e.SumQuantity, QuantityQuality = e.AggregatedQuality }),
                sender,
                receiver);
        }

        private async Task ForwardMessagesOutAsync(IEnumerable<TU> preparedMessages, string type, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var preparedMessage in preparedMessages)
                {
                    await _messageDispatcher.DispatchAsync(preparedMessage, type, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Could not dispatch message due to {error}.", new { error = e.Message });
                throw;
            }
        }
    }
}
