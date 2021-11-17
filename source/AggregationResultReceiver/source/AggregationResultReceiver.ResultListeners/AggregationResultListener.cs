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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.CimXml;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Serialization;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.ResultListeners
{
    public class AggregationResultListener
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ICimXmlResultSerializer _cimXmlResultSerializer;
        private readonly IBlobStore _blobStore;

        public AggregationResultListener(IJsonSerializer jsonSerializer, ICimXmlResultSerializer cimXmlResultSerializer, IBlobStore blobStore)
        {
            _jsonSerializer = jsonSerializer;
            _cimXmlResultSerializer = cimXmlResultSerializer;
            _blobStore = blobStore;
        }

        [Function("AggregationResultListener")]
        public async Task Run(
            [ServiceBusTrigger("mytopic", "mysubscription", Connection = "")] string message,
            FunctionContext context)
        {
            // var messageData = _jsonSerializer.Deserialize<ResultsReadyForConversion>(message);

            // var resultPaths = messageData.resultPaths;
            var resultPaths = new List<string>()
            {
                "result_mock_flex_consumption_per_grid_area.json",
                "result_mock_hourly_consumption_per_grid_area.json",
                "result_mock_net_exchange_per_grid_area.json",
                "result_mock_production_per_grid_area.json",
                "result_mock_total_consumption.json",
            };
            var resultDataJson = new List<string>();
            foreach (var path in resultPaths)
            {
                resultDataJson.Add(await _blobStore.DownloadFromBlobContainerAsync(path, "?", "?").ConfigureAwait(false));
            }

            // List<ResultData> resultData = Deserialize(resulatDataJson);
            // List<XDocument> xmlFiles = _cimXmlResultSerializer.SerializeToStream(resultData, messageData);
        }
    }
}
