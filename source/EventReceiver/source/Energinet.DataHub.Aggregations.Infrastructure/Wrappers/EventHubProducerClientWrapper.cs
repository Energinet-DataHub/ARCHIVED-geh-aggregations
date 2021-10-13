using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace Energinet.DataHub.Aggregations.Infrastructure.Wrappers
{
    public class EventHubProducerClientWrapper : IEventHubProducerClientWrapper
    {
        private readonly EventHubProducerClient _eventHubProducerClient;

        public EventHubProducerClientWrapper(EventHubProducerClient eventHubProducerClient)
        {
            _eventHubProducerClient = eventHubProducerClient;
        }

        public async Task SendAsync(EventDataBatch eventDataBatch, CancellationToken cancellationToken = default)
        {
            await _eventHubProducerClient.SendAsync(eventDataBatch, cancellationToken).ConfigureAwait(false);
        }

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            await _eventHubProducerClient.CloseAsync(cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _eventHubProducerClient.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        public async Task<EventDataBatch> CreateEventBatchAsync(string message, CancellationToken cancellationToken)
        {
            using var eventBatch = await _eventHubProducerClient.CreateBatchAsync(cancellationToken).ConfigureAwait(false);
            var eventData = new EventData(message);

            if (eventBatch.TryAdd(eventData)) return eventBatch;

            throw new InvalidOperationException($"Could not add event data to event batch: {eventData}");
        }
    }
}
