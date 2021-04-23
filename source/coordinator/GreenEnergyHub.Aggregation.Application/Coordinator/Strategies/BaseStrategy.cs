using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public abstract class BaseStrategy<T> : IDispatchStrategy
    {
        private readonly Dispatcher _dispatcher;

        protected BaseStrategy(ILogger<T> logger, Dispatcher dispatcher)
        {
            Logger = logger;
            _dispatcher = dispatcher;
        }

        public abstract string FriendlyNameInstance { get; }

        private protected ILogger<T> Logger { get; }

        public abstract Task DispatchAsync(Stream blobStream, ProcessType pt, string startTime, string endTime, CancellationToken cancellationToken);

        private protected async Task ForwardMessages(IEnumerable<IOutboundMessage> preparedMessages, CancellationToken cancellationToken)
        {
            try
            {
                await _dispatcher.DispatchBulkAsync(preparedMessages, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Could not dispatch message due to {error}", new { error = e.Message });
                throw;
            }
        }
    }
}
