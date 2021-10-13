using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Producer;

namespace Energinet.DataHub.Aggregations.Infrastructure.Wrappers
{
    /// <summary>
    /// Wrapper interface for using EventHubProducerClient
    /// </summary>
    public interface IEventHubProducerClientWrapper : IAsyncDisposable
    {
        // /// <summary>
        // /// Wrapper for creating event data
        // /// </summary>
        // /// <param name="cancellationToken"></param>
        // ValueTask<EventDataBatch> CreateBatchAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Wrapper method for sending event data
        /// </summary>
        /// <param name="eventDataBatch"></param>
        /// <param name="cancellationToken"></param>
        Task SendAsync(EventDataBatch eventDataBatch, CancellationToken cancellationToken = default);

        /// <summary>
        /// Wrapper method for closing connection
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task CloseAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new EventDataBatch and add message to it
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        Task<EventDataBatch> CreateEventBatchAsync(string message, CancellationToken cancellationToken);
    }
}
