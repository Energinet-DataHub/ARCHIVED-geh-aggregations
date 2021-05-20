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
    public class AdjustedProductionStrategyTests : IClassFixture<TestData>
    {
        private readonly TestData _testData;

        public AdjustedProductionStrategyTests(TestData testData)
        {
            _testData = testData;
        }

        [Fact]
        public void Check_Count_Of_HourlyProduction_Handler_Test()
        {
            var hourlyProductionHandler = new AdjustedProductionStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, null, Substitute.For<IGLNService>());

            var list = _testData.HourlyProduction;
            var beginTime = InstantPattern.General.Parse("2020-10-02T09:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T10:00:00Z").GetValueOrThrow();
            var messages = hourlyProductionHandler.PrepareMessages(list,  "D03", beginTime, endTime);

            messages.Should().HaveCount(10);
        }

        [Fact]
        public void Check_Content_Of_HourlyProduction_Message_Test()
        {
            // Arrange
            var hourlyProductionHandler = new AdjustedProductionStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, null, Substitute.For<IGLNService>());
            var list = _testData.HourlyProduction;
            var beginTime = InstantPattern.General.Parse("2020-10-02T11:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T12:00:00Z").GetValueOrThrow();

            // Act
            var messages = hourlyProductionHandler.PrepareMessages(list, "D03", beginTime, endTime)
                .OrderBy(e => ((AggregationResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((AggregationResultMessage)e).BalanceResponsiblePartyMarketParticipantmRID)
                .ThenBy(e => ((AggregationResultMessage)e).BalanceSupplierPartyMarketParticipantmRID)
                .ThenBy(e => ((AggregationResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();

            // Assert
            ((AggregationResultMessage)messages[0]).MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            ((AggregationResultMessage)messages[0]).MeteringGridAreaDomainmRID.Should().Be("500");
            ((AggregationResultMessage)messages[0]).BalanceResponsiblePartyMarketParticipantmRID.Should().Be("8520000000029");
            ((AggregationResultMessage)messages[0]).BalanceSupplierPartyMarketParticipantmRID.Should().Be("8510000000020");
            ((AggregationResultMessage)messages[0]).ReceiverMarketParticipantmRID.Should().Be("8510000000020");
            ((AggregationResultMessage)messages[0]).EnergyObservation.Count().Should().Be(2);
            ((AggregationResultMessage)messages[1]).MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            ((AggregationResultMessage)messages[1]).MeteringGridAreaDomainmRID.Should().Be("500");
            ((AggregationResultMessage)messages[1]).BalanceResponsiblePartyMarketParticipantmRID.Should().Be("8520000000029");
            ((AggregationResultMessage)messages[1]).BalanceSupplierPartyMarketParticipantmRID.Should().Be("8510000000020");
            ((AggregationResultMessage)messages[1]).ReceiverMarketParticipantmRID.Should().Be("8520000000029");
            ((AggregationResultMessage)messages[1]).EnergyObservation.Count().Should().Be(2);
        }
    }
}
