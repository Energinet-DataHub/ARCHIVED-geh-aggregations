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
using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.Interfaces;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Aggregations
{
    public class EventListenerFunction
    {
        private readonly MessageExtractor _messageExtractor;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IJsonSerializer _jsonSerializer;

        public EventListenerFunction(MessageExtractor messageExtractor, IEventDispatcher eventDispatcher, IJsonSerializer jsonSerializer)
        {
            _messageExtractor = messageExtractor;
            _eventDispatcher = eventDispatcher;
            _jsonSerializer = jsonSerializer;
        }

        [Function("EventListenerFunction")]
        public async Task RunAsync(
            [ServiceBusTrigger("%INTEGRATION_EVENT_QUEUE%", Connection = "INTEGRATION_EVENT_QUEUE_CONNECTION")] byte[] data,
            FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var eventName = GetEventName(context);

            var request = await _messageExtractor.ExtractAsync(data).ConfigureAwait(false);
            var serializedMessage = _jsonSerializer.Serialize(request);

            var eventHubMetaData = new Dictionary<string, string>()
            {
                { "EventId", request.Transaction.MRID },
                { "EventName", eventName },
            };

            await _eventDispatcher.DispatchAsync(serializedMessage, eventHubMetaData).ConfigureAwait(false);
        }

        private string GetEventName(FunctionContext context)
        {
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
