using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Infrastruct;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.ServiceBusToEvtHubFunction
{
    public class ServiceBusListenerFunction
    {
        private readonly IConfiguration _configuration;

        public ServiceBusListenerFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("ServiceBusListenerFunction")]
        public async Task RunAsync([ServiceBusTrigger("joules-topic", "joules-sb-subscription", Connection = "ServiceBusConnection")] byte[] msgData, FunctionContext context)
        {
            var logger = context.GetLogger("ServiceBusListenerFunction");
            //logger.LogInformation($"C# ServiceBus queue trigger function processed message: {item}");
            var obj = ChargeLinkCreatedContract.Parser.ParseFrom(msgData);
            logger.LogInformation($"charge id is {obj.ChargeId}");
            var sender = new EventHubService(
                _configuration["EventHubConnectionStringSender"],
                _configuration["EventHubName"]);
            await sender.SendEventHubTestMessage(System.Text.Json.JsonSerializer.Serialize(obj)).ConfigureAwait(false);
        }
    }
}
