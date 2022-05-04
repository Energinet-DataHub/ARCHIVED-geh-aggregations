using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
