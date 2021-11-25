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
using System.Reflection;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Serialization;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.TestHelpers
{
    public static class TestDataGenerator
    {
        public static Stream EmbeddedResourceAssetReader(string fileName)
        {
            var ns = "Energinet.DataHub.Aggregations.AggregationResultReceiver";
            var resource = $"{ns}.Tests.Assets.{fileName}";
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        }

        public static string StreamToString(Stream stream)
        {
            if (stream == null) return string.Empty;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static List<ResultData> GetResultsParameterForMapToCimXml(List<string> list)
        {
            var resultDataList = new List<ResultData>();
            if (list == null) return resultDataList;
            var js = new JsonSerializer();
            foreach (var file in list)
            {
                resultDataList.AddRange(js.DeserializeStream<ResultData>(EmbeddedResourceAssetReader(file)));
            }

            return resultDataList;
        }
    }
}
