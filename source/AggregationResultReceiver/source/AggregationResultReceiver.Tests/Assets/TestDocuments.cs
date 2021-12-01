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
using System.Xml.Linq;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Serialization;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.Assets
{
    public class TestDocuments
    {
        private readonly JsonSerializer _jsonSerializer;

        public TestDocuments()
        {
            _jsonSerializer = new JsonSerializer();
        }

        public XDocument ExpectedAggregationResultForPerGridAreaMdr501 => GetDocumentAsXDocument("ExpectedAggregationResultForPerGridAreaMdr501.xml.blob");

        public XDocument ExpectedAggregationResultForPerGridAreaMdr502 => GetDocumentAsXDocument("ExpectedAggregationResultForPerGridAreaMdr502.xml.blob");

        public string JobCompletedEvent => GetDocumentAsString("job_completed_event.json");

        public Stream FlexConsumptionPerGridArea => GetDocumentStream("result_mock_flex_consumption_per_grid_area.json");

        public Stream HourlyConsumptionPerGridArea => GetDocumentStream("result_mock_hourly_consumption_per_grid_area.json");

        public Stream NetExchangePerGridArea => GetDocumentStream("result_mock_net_exchange_per_grid_area.json");

        public Stream ProductionPerGridArea => GetDocumentStream("result_mock_production_per_grid_area.json");

        public Stream TotalConsumptionPerGridArea => GetDocumentStream("result_mock_total_consumption.json");

        public IEnumerable<ResultData> AggregationResultsForMdr()
        {
            foreach (var resultData in _jsonSerializer.DeserializeMultipleContent<ResultData>(FlexConsumptionPerGridArea))
                yield return resultData;
            foreach (var resultData in _jsonSerializer.DeserializeMultipleContent<ResultData>(HourlyConsumptionPerGridArea))
                yield return resultData;
            foreach (var resultData in _jsonSerializer.DeserializeMultipleContent<ResultData>(NetExchangePerGridArea))
                yield return resultData;
            foreach (var resultData in _jsonSerializer.DeserializeMultipleContent<ResultData>(ProductionPerGridArea))
                yield return resultData;
            foreach (var resultData in _jsonSerializer.DeserializeMultipleContent<ResultData>(TotalConsumptionPerGridArea))
                yield return resultData;
        }

        private XDocument GetDocumentAsXDocument(string documentName)
        {
            return XDocument.Parse(GetDocumentAsString(documentName));
        }

        private Stream GetDocumentStream(string documentName)
        {
            var rootNamespace = GetType().Namespace;
            return GetType().Assembly.GetManifestResourceStream($"{rootNamespace}.{documentName}");
        }

        private string GetDocumentAsString(string documentName)
        {
            var stream = GetDocumentStream(documentName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
