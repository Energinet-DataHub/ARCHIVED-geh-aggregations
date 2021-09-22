using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.ChargeLink;
using GreenEnergyHub.Aggregation.Infrastruct;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.ServiceBusToEvtHubFunction
{
    public class ServiceBusListenerFunction
    {
        private readonly IEventDispatcher _dispatcher;

        public ServiceBusListenerFunction(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [Function("ServiceBusListenerFunction")]
        public async Task RunAsync([ServiceBusTrigger("joules-topic", "joules-sb-subscription", Connection = "ServiceBusConnection")] byte[] msgData, FunctionContext context)
        {
            var logger = context.GetLogger("ServiceBusListenerFunction");
            logger.LogInformation($"ServiceBusListenerFunction triggered");
            await _dispatcher.DispatchAsync(msgData).ConfigureAwait(false);
        }
    }
}
