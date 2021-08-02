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

using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Aggregation.Application.Coordinator.Strategies;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.ResultMessages;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Tests.Assets;
using Microsoft.Extensions.Logging;
using NodaTime.Text;
using NSubstitute;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
{
    [Trait("Category", "Component")]
    public class ExchangeStrategiesTests : IClassFixture<TestData>
    {
        private readonly TestData _testData;
        private readonly GlnService _glnService;

        public ExchangeStrategiesTests(TestData testData)
        {
            _testData = testData;
            _glnService = new GlnService("datahub_gln", "esett_gln");
        }

        [Fact]
        public void Check_Content_Of_ExchangeGa_Message_Test()
        {
            // Arrange
            var testData = _testData.ExchangeGa;
            var exchangeStrategy = new Step02ExchangeStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, _glnService);
            var beginTime = InstantPattern.General.Parse("2020-10-03T07:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T08:00:00Z").GetValueOrThrow();

            // Act
            var message = (AggregationResultMessage)exchangeStrategy.PrepareMessages(testData.ToList(), "D03", beginTime, endTime).First();

            // Assert
            message.EnergyObservation.Should().HaveCount(2); // there should be only 2 EnergyObservations in grid area 500, see Exchange.json
            message.Kind.ShouldBeEquivalentTo(23);
            message.MeteringGridAreaDomainmRID.ShouldBeEquivalentTo("500");
            message.TimeIntervalStart.ShouldBeEquivalentTo(beginTime);
            message.TimeIntervalEnd.ShouldBeEquivalentTo(endTime);
            message.EnergyObservation.First().EnergyQuantity.Should().Be(-32.000m);
            message.EnergyObservation.First().QuantityQuality.Should().Be(Quality.Estimated);
        }

        [Fact]
        public void Check_Content_Of_ExchangeNeighbour_Message_Test()
        {
            // Arrange
            var testData = _testData.ExchangeNeighbour;
            var exchangeStrategy = new Step01ExchangePerNeighbourStrategy(Substitute.For<ILogger<ExchangeNeighbourDto>>(), null, _glnService);
            var beginTime = InstantPattern.General.Parse("2020-10-03T07:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T08:00:00Z").GetValueOrThrow();

            // Act
            var message = (AggregatedExchangeNeighbourResultMessage)exchangeStrategy.PrepareMessages(testData, "D03", beginTime, endTime).First();

            // Assert
            message.EnergyObservation.Should().HaveCount(2); // there should be only 2 EnergyObservations in grid area 500, see ExchangeNeighbour.json
            message.Kind.ShouldBeEquivalentTo(23);
            message.MeteringGridAreaDomainmRID.ShouldBeEquivalentTo("500");
            message.InMeteringGridAreaDomainmRID.ShouldBeEquivalentTo("500");
            message.OutMeteringGridAreaDomainmRID.ShouldBeEquivalentTo("501");
            message.TimeIntervalStart.ShouldBeEquivalentTo(beginTime);
            message.TimeIntervalEnd.ShouldBeEquivalentTo(endTime);
            message.EnergyObservation.First().EnergyQuantity.Should().Be(-32.000m);
            message.EnergyObservation.First().QuantityQuality.Should().Be(Quality.Estimated);
        }
    }
}
