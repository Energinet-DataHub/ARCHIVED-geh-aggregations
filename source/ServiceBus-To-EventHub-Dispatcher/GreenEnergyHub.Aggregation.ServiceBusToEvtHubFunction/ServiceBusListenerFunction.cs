using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.ServiceBusToEvtHubFunction
{
    public class ServiceBusListenerFunction
    {
        private readonly IEventDispatcher _dispatcher;
        private readonly ILogger<ServiceBusListenerFunction> _logger;

        public ServiceBusListenerFunction(IEventDispatcher dispatcher, ILogger<ServiceBusListenerFunction> logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;
        }

        [Function("charge-link-created")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                    "%charge-link-created-topic%",
                    "%ServiceBusSubscription%",
                    Connection = "ServiceBusConnection")]
            byte[] msgData,
            FunctionContext context)
        {
            var contractName = context.FunctionDefinition.Name;
            _logger.LogInformation($"ServiceBusListenerFunction triggered for {contractName}");
            await _dispatcher.DispatchAsync(msgData, contractName).ConfigureAwait(false);
        }
    }
}
