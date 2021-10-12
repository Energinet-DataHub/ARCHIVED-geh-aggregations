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
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Energinet.DataHub.Aggregations.Infrastructure.Wrappers;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Aggregations.Infrastructure
{
    public sealed class EventHubService : IEventHubService
    {
        private readonly ILogger<EventHubService> _logger;
        private readonly IEventHubProducerClientWrapper _eventHubProducerClient;

        public EventHubService(IEventHubProducerClientWrapper eventHubProducerClient, ILogger<EventHubService> logger)
        {
            _eventHubProducerClient = eventHubProducerClient;
            _logger = logger;
        }

        public async Task SendEventHubMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            try
            {
                var eventDataBatch = await _eventHubProducerClient.CreateEventBatchAsync(message, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Sending message onto eventhub");
                await _eventHubProducerClient.SendAsync(eventDataBatch, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // Transient failures will be automatically retried as part of the
                // operation. If this block is invoked, then the exception was either
                // fatal or all retries were exhausted without a successful publish.
                _logger.LogError("Failed sending event hub message " + e.Message);
                throw;
            }
            finally
            {
                await _eventHubProducerClient.CloseAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _eventHubProducerClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}
