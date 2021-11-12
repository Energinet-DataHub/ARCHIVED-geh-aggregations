// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Serialization;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.ResultListeners
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
