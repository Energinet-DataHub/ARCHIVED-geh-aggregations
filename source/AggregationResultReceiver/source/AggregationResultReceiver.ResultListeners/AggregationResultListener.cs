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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.CimXml;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Converters;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Serialization;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.ResultListeners
{
    public class AggregationResultListener
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ICimXmlResultSerializer _cimXmlResultSerializer;
        private readonly IBlobStore _blobStore;
        private readonly ICimXmlConverter _cimXmlConverter;

        public AggregationResultListener(IJsonSerializer jsonSerializer, ICimXmlResultSerializer cimXmlResultSerializer, IBlobStore blobStore, ICimXmlConverter cimXmlConverter)
        {
            _jsonSerializer = jsonSerializer;
            _cimXmlResultSerializer = cimXmlResultSerializer;
            _blobStore = blobStore;
            _cimXmlConverter = cimXmlConverter;
        }

        [Function("AggregationResultListener")]
        public async Task Run(
            [ServiceBusTrigger("mytopic", "mysubscription", Connection = "")] string message,
            FunctionContext context)
        {
            var messageData = _jsonSerializer.Deserialize<JobCompletedEvent>(message);
            var resultDataJson = new List<string>();
            foreach (var result in messageData.Results)
            {
                resultDataJson.Add(await _blobStore.DownloadFromBlobContainerAsync("?", "?", result.ResultPath).ConfigureAwait(false));
            }

            List<ResultData> resultDataList = new List<ResultData>(); // deserialize resultDataJson to resultDataList
            var outgoingResults = _cimXmlConverter.Convert(resultDataList, messageData);
            foreach (var result in outgoingResults)
            {
                var stream = new MemoryStream();
                await result.Document.SaveAsync(stream, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);
                stream.Position = 0;
                await _blobStore.UploadStreamToBlobContainerAsync("?", "?", result.ResultId, stream).ConfigureAwait(false);
            }
        }
    }
}
