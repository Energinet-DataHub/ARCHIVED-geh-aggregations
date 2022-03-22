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
using Energinet.DataHub.Aggregations.Application.Extensions;
using Energinet.DataHub.Aggregations.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Common
{
    public class EventDataHelper
    {
        private readonly IJsonSerializer _jsonSerializer;

        public EventDataHelper(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public EventMetadata GetEventMetaData(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.BindingContext.BindingData.TryGetValue("UserProperties", out var metadata);

            if (metadata is null)
            {
                throw new InvalidOperationException($"Service bus metadata must be specified as User Properties attributes");
            }

            var eventMetadata = _jsonSerializer.Deserialize<EventMetadata>(metadata.ToString() ?? throw new InvalidOperationException());

            if (eventMetadata != null)
            {
                if (string.IsNullOrWhiteSpace(eventMetadata.eventidentification))
                {
                    throw new ArgumentException("EventIdentification is not set");
                }

                if (string.IsNullOrWhiteSpace(eventMetadata.messagetype))
                {
                    throw new ArgumentException("MessageType is not set");
                }

                if (string.IsNullOrWhiteSpace(eventMetadata.operationcorrelationId))
                {
                    throw new ArgumentException("OperationCorrelationId is not set");
                }

                if (eventMetadata.messageversion < 1)
                {
                    throw new ArgumentException("MessageVersion is not set");
                }

                if (eventMetadata.operationtimestamp == Instant.MinValue)
                {
                    throw new ArgumentException("OperationTimestamp is not set");
                }

                return eventMetadata;
            }

            throw new InvalidOperationException("Service bus metadata is null");
        }

        public Dictionary<string, string> GetEventhubMetaData(EventMetadata eventMetaData, string domain)
        {
            if (eventMetaData == null)
            {
                throw new ArgumentNullException(nameof(eventMetaData));
            }

            return new Dictionary<string, string>
            {
                { "event_id", eventMetaData.eventidentification },
                { "processed_date", eventMetaData.operationtimestamp.ToIso8601GeneralString() },
                { "event_name", eventMetaData.messagetype },
                { "domain", domain },
            };
        }
    }
}
