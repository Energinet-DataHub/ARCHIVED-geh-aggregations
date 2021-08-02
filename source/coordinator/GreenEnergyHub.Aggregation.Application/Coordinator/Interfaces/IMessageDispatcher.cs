using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces
{
    /// <summary>
    /// Dispatches messages out of the application
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Utility method for deserializing with custom nodatimehandling
        /// </summary>
        /// <typeparam name="T">The T to deserialize to</typeparam>
        /// <param name="str"></param>
        /// <returns>Deserialized object</returns>
        public T Deserialize<T>(string str);

        /// <summary>
        /// Send a <paramref name="message"/> to a channel
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="type">Type of message</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <exception cref="ArgumentNullException">message is <c>null</c></exception>
        public Task DispatchAsync<T>(T message, string type, CancellationToken cancellationToken)
            where T : IOutboundMessage;
    }
}
