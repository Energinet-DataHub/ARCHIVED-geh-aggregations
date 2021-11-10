using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Aggregations.ResultReceiver.ResultListeners
{
    public static class AggregationResultListener
    {
        [Function("AggregationResultListener")]
        public static void Run(
            [ServiceBusTrigger("mytopic", "mysubscription", Connection = "")] string message,
            FunctionContext context)
        {
        }
    }
}
