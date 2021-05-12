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
using System.Threading;
using System.Threading.Tasks;
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
        private readonly MessageDispatcher _dispatcher;

        protected BaseStrategy(ILogger<T> logger, MessageDispatcher dispatcher, IJsonSerializer jsonSerializer)
        {
            Logger = logger;
            _dispatcher = dispatcher;
            _jsonSerializer = jsonSerializer;
        }

        private protected ILogger<T> Logger { get; }

        public virtual async Task DispatchAsync(Stream blobStream, ProcessType pt, Instant startTime, Instant endTime, CancellationToken cancellationToken)
        {
            var listOfResults = await _jsonSerializer.DeserializeAsync<IEnumerable<T>>(blobStream, cancellationToken).ConfigureAwait(false);

            var messages = PrepareMessages(listOfResults, pt, startTime, endTime);

            await ForwardMessagesOutAsync(messages, cancellationToken).ConfigureAwait(false);
        }

        public abstract IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<T> aggregationResultList, ProcessType processType, Instant timeIntervalStart, Instant timeIntervalEnd);

        private async Task ForwardMessagesOutAsync(IEnumerable<IOutboundMessage> preparedMessages, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var preparedMessage in preparedMessages)
                {
                    await _dispatcher.DispatchAsync(preparedMessage, cancellationToken).ConfigureAwait(false);
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
