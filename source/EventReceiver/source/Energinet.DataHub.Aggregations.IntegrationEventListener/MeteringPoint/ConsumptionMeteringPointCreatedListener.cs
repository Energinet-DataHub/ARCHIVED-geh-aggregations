using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.Interfaces;
using Energinet.DataHub.Aggregations.Common;
using Energinet.DataHub.Aggregations.Infrastructure.Messaging;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Aggregations.MeteringPoint
{
    public class ConsumptionMeteringPointCreatedListener
    {
        private readonly MessageExtractor<ConsumptionMeteringPointCreated> _messageExtractor;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly EventDataHelper _eventDataHelper;

        public ConsumptionMeteringPointCreatedListener(MessageExtractor<ConsumptionMeteringPointCreated> messageExtractor, IEventDispatcher eventDispatcher, EventDataHelper eventDataHelper)
        {
            _messageExtractor = messageExtractor;
            _eventDispatcher = eventDispatcher;
            _eventDataHelper = eventDataHelper;
        }

        [Function("ConsumptionMeteringPointCreatedListener")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME%",
                "%CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME%",
                Connection = "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING")] byte[] data,
            FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var eventName = _eventDataHelper.GetEventName(context);
            var request = await _messageExtractor.ExtractAsync(data).ConfigureAwait(false);
            var eventHubMetaData = new Dictionary<string, string>()
            {
                { "EventId", request.Transaction.MRID },
                { "EventName", eventName },
            };

            await _eventDispatcher.DispatchAsync(request, eventHubMetaData).ConfigureAwait(false);
        }
    }
}
