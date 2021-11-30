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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture.Xunit2;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Helpers;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain.Enums;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Converters;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.Assets;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.Attributes;
using FluentAssertions;
using Moq;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests
{
    [UnitTest]
    public class CimXmlConverterTests
    {
        [Theory]
        [AutoMoqData]
        public void CimXmlConverter_ValidInput_ReturnsCorrectsXml(
            [NotNull] TestDocuments testDocuments,
            [NotNull][Frozen] Mock<IGuidGenerator> guidGenerator,
            [NotNull][Frozen] Mock<IInstantGenerator> instantGenerator,
            [NotNull] CimXmlConverter sut)
        {
            // Arrange
            guidGenerator.Setup(g => g.GetGuidAsStringOnlyDigits())
                .Returns("4514559a-7311-431a-a8c0-210ccc8ce003");
            instantGenerator.Setup(i => i.GetCurrentDateTimeUtc())
                .Returns(InstantPattern.General.Parse("2021-11-12T08:11:48Z").Value);
            var jobCompletedEvent = new JobCompletedEvent(
                ProcessType.BalanceFixing,
                ProcessVariant.FirstRun,
                Resolution.Hourly,
                It.IsAny<IEnumerable<AggregationResult>>(),
                InstantPattern.General.Parse("2021-09-05T22:00:00Z").Value,
                InstantPattern.General.Parse("2021-09-06T22:00:00Z").Value);
            var resultDataList = testDocuments.AggregationResultsForMdr();
            var expected = testDocuments.ExpectedAggregationResultForPerGridAreaMdr501;

            // Act
            var xmlFiles = sut.Convert(resultDataList, jobCompletedEvent);
            var actual = xmlFiles.First().Document;

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
