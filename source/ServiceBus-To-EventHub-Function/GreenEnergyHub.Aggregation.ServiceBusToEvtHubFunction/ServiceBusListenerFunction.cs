using System;
using System.IO;
using System.Linq;
using System.Text;
using GreenEnergyHub.Aggregation.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.ServiceBusToEvtHubFunction
{
    public static class ServiceBusListenerFunction
    {
        [Function("ServiceBusListenerFunction")]
        public static void Run([ServiceBusTrigger("joules-topic", "joules-sb-subscription", Connection = "ServiceBusConnection")] byte[] msgData, FunctionContext context)
        {
            var logger = context.GetLogger("ServiceBusListenerFunction");
            //logger.LogInformation($"C# ServiceBus queue trigger function processed message: {item}");
            var obj = ChargeLinkCreatedContract.Parser.ParseFrom(msgData);
            logger.LogInformation($"charge id is {obj.ChargeId}");
        }
    }
}
