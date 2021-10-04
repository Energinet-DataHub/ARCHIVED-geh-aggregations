using System;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Aggregations
{
    public static class ConsumptionMeteringPointCreatedFunction
    {
        [Function("ConsumptionMeteringPointCreatedFunction")]
        public static void Run(
            [ServiceBusTrigger(
                "consumption-metering-point-created",
                "consumption-metering-point-created-sub-aggregations",
                Connection = "ServiceBusConnection")] byte[] data,
            FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (data == null) throw new ArgumentNullException(nameof(data));
        }
    }
}
