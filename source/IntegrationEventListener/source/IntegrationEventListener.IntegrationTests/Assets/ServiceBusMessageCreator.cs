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
using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Assets
{
    internal class ServiceBusMessageCreator
    {
        public static ServiceBusMessage CreateSbMessage(
            DateTime effectiveDate,
            byte[] message,
            string operationCorrelationId,
            string eventId,
            string msgType)
        {
            var serviceBusMessage = new ServiceBusMessage(message);
            serviceBusMessage.ApplicationProperties.Add("OperationTimestamp", effectiveDate.ToUniversalTime());
            serviceBusMessage.ApplicationProperties.Add("OperationCorrelationId", operationCorrelationId);
            serviceBusMessage.ApplicationProperties.Add("MessageVersion", 1);
            serviceBusMessage.ApplicationProperties.Add("MessageType", msgType);
            serviceBusMessage.ApplicationProperties.Add("EventIdentification", eventId);
            return serviceBusMessage;
        }
    }
}
