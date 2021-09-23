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
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using GreenEnergyHub.Aggregation.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Infrastruct
{
    public class EventHubService : IEventHubService, IDisposable, IAsyncDisposable
    {
        private readonly ILogger<EventHubService> _logger;
        private EventHubProducerClient _producerClient;

        public EventHubService(IConfiguration configuration, ILogger<EventHubService> logger)
        {
            if (configuration == null)
            {
                throw new ArgumentException($"configuration is null");
            }

            _logger = logger;

            _producerClient = new EventHubProducerClient(
                configuration["EventHubConnectionStringSender"], configuration["EventHubName"]);
        }

        public async Task SendEventHubMessageAsync(byte[] msg)
        {
            try
            {
                using var eventBatch = await _producerClient.CreateBatchAsync().ConfigureAwait(false);

                _logger.LogInformation("Sending message onto eventhub");
                var eventBody = new BinaryData(msg);
                var eventData = new EventData(eventBody);

                if (!eventBatch.TryAdd(eventData))
                {
                    _logger.LogError("Failed adding message to batch");
                }

                await _producerClient.SendAsync(eventBatch).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed sending event hub message " + e.Message);
                // Transient failures will be automatically retried as part of the
                // operation. If this block is invoked, then the exception was either
                // fatal or all retries were exhausted without a successful publish.
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            Dispose(disposing: false);
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_producerClient as IDisposable)?.Dispose();
            }

            _producerClient = null;
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_producerClient != null)
            {
                await _producerClient.DisposeAsync().ConfigureAwait(false);
            }

            _producerClient = null;
        }
    }
}
