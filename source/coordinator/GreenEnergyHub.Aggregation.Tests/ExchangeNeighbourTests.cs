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
    public class ExchangeNeighbourTests : IClassFixture<TestData>
    {
        private readonly TestData _testData;

        public ExchangeNeighbourTests(TestData testData)
        {
            _testData = testData;
        }

        [Fact]
        public void Check_Content_Of_ExchangeNeighbour_Message_Test()
        {
            // Arrange
            var testData = _testData.ExchangeNeighbour;
            var exchangeStrategy = new ExchangeNeighbourStrategy(Substitute.For<ILogger<ExchangeNeighbourDto>>(), null, null, Substitute.For<IGLNService>());
            var beginTime = InstantPattern.General.Parse("2020-10-03T07:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T08:00:00Z").GetValueOrThrow();

            // Act
            var message = (AggregatedExchangeNeighbourResultMessage)exchangeStrategy.PrepareMessages(testData, "D03", beginTime, endTime).First();

            // Assert
            message.Kind.ShouldBeEquivalentTo(23);
            message.MeteringGridAreaDomainmRID.ShouldBeEquivalentTo("500");
            message.InMeteringGridAreaDomainmRID.ShouldBeEquivalentTo("500");
            message.OutMeteringGridAreaDomainmRID.ShouldBeEquivalentTo("501");
            message.TimeIntervalStart.ShouldBeEquivalentTo(beginTime);
            message.TimeIntervalEnd.ShouldBeEquivalentTo(endTime);
            message.EnergyObservation.First().EnergyQuantity.Should().Be(-32.000);
            message.EnergyObservation.First().QuantityQuality.Should().Be(Quality.Estimated);
        }
    }
}
