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

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Serialization;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.CimXml;
using Newtonsoft.Json;
using NodaTime.Text;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests
{
    [UnitTest]
    public class CimXmlResultSerializerTests
    {
        private readonly CimXmlResultSerializer _sut;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IInstantGenerator _instantGenerator;

        public CimXmlResultSerializerTests()
        {
            _guidGenerator = Substitute.For<IGuidGenerator>();
            _instantGenerator = Substitute.For<IInstantGenerator>();
            _sut = new CimXmlResultSerializer(_guidGenerator, _instantGenerator);
        }

        [Fact]
        public void MapToCimXml_ValidInput_ReturnsCorrectsXml()
        {
            _guidGenerator.GetGuid().Returns(Guid.Parse("4514559a-7311-431a-a8c0-210ccc8ce003"));
            _instantGenerator.GetCurrentDateTimeUtc().Returns(InstantPattern.General.Parse("2021-11-12T08:11:48Z").Value);
            var xmlAsString = EmbeddedResourceAssetReader("ExpectedAggregationResultForPerGridAreaMdr501.xml");
            var expected = XDocument.Parse(xmlAsString).ToString();
            var actual = GetResultData().ToString();
            Assert.Equal(expected, actual);
        }

        private string EmbeddedResourceAssetReader(string fileName)
        {
            var resource = $"Energinet.DataHub.AggregationResultReceiver.Tests.Assets.{fileName}";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            if (stream == null) return string.Empty;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private List<ResultData> JsonMultipleContentReader(string jsonContent)
        {
            var resultDataArray = new List<ResultData>();
            JsonTextReader reader = new JsonTextReader(new StringReader(jsonContent));
            reader.SupportMultipleContent = true;
            JsonSerializer serializer = new JsonSerializer();
            while (true)
            {
                if (!reader.Read())
                {
                    break;
                }

                ResultData resultData = serializer.Deserialize<ResultData>(reader);

                resultDataArray.Add(resultData);
            }

            return resultDataArray;
        }

        private XDocument GetResultData()
        {
            var list = new List<string>()
            {
                "result_mock_flex_consumption_per_grid_area.json",
                "result_mock_hourly_consumption_per_grid_area.json",
                "result_mock_net_exchange_per_grid_area.json",
                "result_mock_production_per_grid_area.json",
                "result_mock_total_consumption.json",
            };
            var resultDataArray = new List<ResultData>();

            foreach (var file in list)
            {
                resultDataArray.AddRange(JsonMultipleContentReader(EmbeddedResourceAssetReader(file)));
            }

            var xmlFiles = _sut.MapToCimXml(resultDataArray);
            return xmlFiles[0];
        }
    }
}
