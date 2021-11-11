using System.Collections.Generic;
using System.Linq;
using AggregationResultReceiver.Application.Serialization;
using AggregationResultReceiver.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Aggregations.ResultReceiver.ResultListeners
{
    public class AggregationResultListener
    {
        private readonly IJsonSerializer _jsonSerializer;

        public AggregationResultListener(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        [Function("AggregationResultListener")]
        public void Run(
            [ServiceBusTrigger("mytopic", "mysubscription", Connection = "")] string message,
            FunctionContext context)
        {
            // var results = _jsonSerializer.Deserialize<List<ResultsReadyForConversion>>(message);
            // var resultPaths = results.Select(x => x.ResultPath).ToList();
            // List<ResultData> resultData = _dataAccessLayer.GetResultDataFromBlob(resultPaths);
            // MapToCimXml(resultData);
        }
    }
}
