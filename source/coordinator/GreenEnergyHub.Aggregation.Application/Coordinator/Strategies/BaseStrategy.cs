using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public abstract class BaseStrategy<T>
    {
        private readonly Dispatcher _dispatcher;

        protected BaseStrategy(ILogger<T> logger, Dispatcher dispatcher)
        {
            Logger = logger;
            _dispatcher = dispatcher;
        }

        public abstract string FriendlyNameInstance { get; }

        private protected ILogger<T> Logger { get; }

        public virtual async Task DispatchAsync(Stream blobStream, ProcessType pt, string startTime, string endTime, CancellationToken cancellationToken)
        {
            var listOfResults = await JsonSerializer.DeserializeAsync<IEnumerable<T>>(blobStream, cancellationToken: cancellationToken).ConfigureAwait(false);

            var messages = PrepareMessages(listOfResults, pt, startTime, endTime);

            await ForwardMessagesOut(messages, cancellationToken).ConfigureAwait(false);
        }

        public abstract IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<T> list, ProcessType processType, string timeIntervalStart, string timeIntervalEnd);

        private protected async Task ForwardMessagesOut(IEnumerable<IOutboundMessage> preparedMessages, CancellationToken cancellationToken)
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
