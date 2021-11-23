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
using Newtonsoft.Json;
using Xunit;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.TestHelpers
{
    public static class TestDataGenerator
    {
        public static string EmbeddedResourceAssetReader(string fileName)
        {
            var ns = "Energinet.DataHub.Aggregations.AggregationResultReceiver";
            var resource = $"{ns}.Tests.Assets.{fileName}";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            if (stream == null) return string.Empty;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static List<ResultData> GetResultsParameterForMapToCimXml(List<string> list)
        {
            var resultDataList = new List<ResultData>();
            if (list == null) return resultDataList;
            foreach (var file in list)
            {
                resultDataList.AddRange(JsonMultipleContentReader(EmbeddedResourceAssetReader(file)));
            }

            return resultDataList;
        }

        private static List<ResultData> JsonMultipleContentReader(string jsonContent)
        {
            var resultDataArray = new List<ResultData>();
            using var reader = new JsonTextReader(new StringReader(jsonContent));
            reader.SupportMultipleContent = true;
            var serializer = new JsonSerializer();
            while (true)
            {
                if (!reader.Read())
                {
                    break;
                }

                var resultData = serializer.Deserialize<ResultData>(reader);

                resultDataArray.Add(resultData);
            }

            return resultDataArray;
        }
    }
}
