using System;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace GreenEnergyHub.Aggregation.Infrastruct
{
    public class EventHubService
    {
        private EventHubProducerClient _producer;

        public EventHubService(string eventHubConnectionString, string eventHubName)
        {
            _producer = new EventHubProducerClient(eventHubConnectionString, eventHubName);
        }

        public async Task SendEventHubTestMessage(string message)
        {
            try
            {
                using EventDataBatch eventBatch = await _producer.CreateBatchAsync().ConfigureAwait(false);
                Console.WriteLine("Converting message");
                var eventBody = new BinaryData(message);
                var eventData = new EventData(eventBody);

                if (!eventBatch.TryAdd(eventData))
                {
                    Console.WriteLine("Failed adding message to batch");
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
                Console.WriteLine("Sending message");
                await _producer.SendAsync(eventBatch).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed sending event hub message " + e.Message);
                // Transient failures will be automatically retried as part of the
                // operation. If this block is invoked, then the exception was either
                // fatal or all retries were exhausted without a successful publish.
            }
        }
    }
}
