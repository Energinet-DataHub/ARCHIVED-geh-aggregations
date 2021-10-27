using System;
using Energinet.DataHub.Aggregations.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Aggregations.Common
{
    public class EventDataHelper
    {
        private readonly IJsonSerializer _jsonSerializer;

        public EventDataHelper(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public string GetEventName(FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.BindingContext.BindingData.TryGetValue("UserProperties", out var metadata);

            if (metadata is null)
            {
                throw new InvalidOperationException($"Service bus metadata must be specified as User Properties attributes");
            }

            var eventMetadata = _jsonSerializer.Deserialize<EventMetadata>(metadata.ToString() ?? throw new InvalidOperationException());
            return eventMetadata.MessageType ?? throw new InvalidOperationException("Service bus metadata property MessageType is missing");
        }
    }
}
