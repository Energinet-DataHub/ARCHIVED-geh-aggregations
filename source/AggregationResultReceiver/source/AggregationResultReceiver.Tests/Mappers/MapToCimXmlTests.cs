﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Linq;
using System.Xml.Linq;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Helpers;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.CimXml;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Converters;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.Helpers;
using NodaTime.Text;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.Mappers
{
    [UnitTest]
    public class MapToCimXmlTests
    {
        private readonly CimXmlConverter _sut;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IInstantGenerator _instantGenerator;

        public MapToCimXmlTests()
        {
            _guidGenerator = Substitute.For<IGuidGenerator>();
            _instantGenerator = Substitute.For<IInstantGenerator>();
            _sut = new CimXmlConverter(_guidGenerator, _instantGenerator);
        }

        [Fact]
        public void MapToCimXml_ValidInput_ReturnsCorrectsXml()
        {
            // Arrange
            _guidGenerator.GetGuid().Returns("4514559a-7311-431a-a8c0-210ccc8ce003");
            _instantGenerator.GetCurrentDateTimeUtc()
                .Returns(InstantPattern.General.Parse("2021-11-12T08:11:48Z").Value);
            var testDataGenerator = new TestDataGenerator();
            var resultDataList = testDataGenerator.GetResultsParameterForMapToCimXml();

            // Act
            var xmlFiles = _sut.Convert(resultDataList, null);
            var xmlAsString =
                testDataGenerator.EmbeddedResourceAssetReader("ExpectedAggregationResultForPerGridAreaMdr501.xml");
            var expected = XDocument.Parse(xmlAsString).ToString();
            var actual = xmlFiles.First().Document.ToString();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
