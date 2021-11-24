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
using System.Linq;
using System.Xml.Linq;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Helpers;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain.Enums;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Converters;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.TestHelpers;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.Converters
{
    [UnitTest]
    public class CimXmlConverterTests
    {
        private readonly CimXmlConverter _sut;
        private readonly Mock<IGuidGenerator> _guidGenerator;
        private readonly Mock<IInstantGenerator> _instantGenerator;

        public CimXmlConverterTests()
        {
            _guidGenerator = new Mock<IGuidGenerator>();
            _instantGenerator = new Mock<IInstantGenerator>();
            _sut = new CimXmlConverter(_guidGenerator.Object, _instantGenerator.Object);
        }

        [Fact]
        public void CimXmlConverter_ValidInput_ReturnsCorrectsXml()
        {
            // Arrange
            _guidGenerator.Setup(g => g.GetGuid()).Returns("4514559a-7311-431a-a8c0-210ccc8ce003");
            _instantGenerator.Setup(i => i.GetCurrentDateTimeUtc())
                .Returns(InstantPattern.General.Parse("2021-11-12T08:11:48Z").Value);
            var list = new List<string>()
            {
                "result_mock_flex_consumption_per_grid_area.json",
                "result_mock_hourly_consumption_per_grid_area.json",
                "result_mock_net_exchange_per_grid_area.json",
                "result_mock_production_per_grid_area.json",
                "result_mock_total_consumption.json",
            };
            var resultDataList = TestDataGenerator.GetResultsParameterForMapToCimXml(list);

            var messageData = new JobCompletedEvent(
                " ",
                " ",
                ProcessType.BalanceFixing,
                ProcessVariant.FirstRun,
                Resolution.Hourly,
                new List<AggregationResult>() { new AggregationResult(" ", " ", Grouping.GridArea) },
                Instant.FromDateTimeUtc(DateTime.UtcNow),
                Instant.FromDateTimeUtc(DateTime.UtcNow));

            // Act
            var xmlFiles = _sut.Convert(resultDataList, messageData);
            var xmlAsString =
                TestDataGenerator.EmbeddedResourceAssetReader("ExpectedAggregationResultForPerGridAreaMdr501.xml");
            var expected = XDocument.Parse(xmlAsString).ToString();
            var actual = xmlFiles.First().Document.ToString();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
