using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.Interfaces;
using Energinet.DataHub.Aggregations.Common;
using Energinet.DataHub.Aggregations.Infrastructure.Messaging;
using Energinet.DataHub.MarketRoles.IntegrationEventContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Aggregations.MarketRoles
{
    public class EnergySupplierChangedListener
    {
        private readonly MessageExtractor<EnergySupplierChanged> _messageExtractor;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly EventDataHelper _eventDataHelper;

        public EnergySupplierChangedListener(MessageExtractor<EnergySupplierChanged> messageExtractor, IEventDispatcher eventDispatcher, EventDataHelper eventDataHelper)
        {
            _messageExtractor = messageExtractor;
            _eventDispatcher = eventDispatcher;
            _eventDataHelper = eventDataHelper;
        }

        [Function("EnergySupplierChangedListener")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%ENERGY_SUPPLIER_CHANGED_TOPIC_NAME%",
                "%ENERGY_SUPPLIER_CHANGED_SUBSCRIPTION_NAME%",
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
