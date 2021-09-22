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

            _producerClient = new EventHubProducerClient(configuration["EventHubConnectionStringSender"], configuration["EventHubName"]);
        }

        public async Task SendEventHubTestMessageAsync(string message)
        {
            try
            {
                using var eventBatch = await _producerClient.CreateBatchAsync().ConfigureAwait(false);

                _logger.LogInformation("Sending message onto eventhub");
                var eventBody = new BinaryData(message);
                var eventData = new EventData(eventBody);

                if (!eventBatch.TryAdd(eventData))
                {
                    _logger.LogError("Failed adding message to batch");
                    // At this point, the batch is full but our last event was not
                    // accepted.  For our purposes, the event is unimportant so we
                    // will intentionally ignore it.  In a real-world scenario, a
                    // decision would have to be made as to whether the event should
                    // be dropped or published on its own.
                }

                // When the producer publishes the event, it will receive an
                // acknowledgment from the Event Hubs service; so long as there is no
                // exception thrown by this call, the service assumes responsibility for
                // delivery.  Your event data will be published to one of the Event Hub
                // partitions, though there may be a (very) slight delay until it is
                // available to be consumed.
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
