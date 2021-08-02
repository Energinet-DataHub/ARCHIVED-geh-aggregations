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
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
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
    public class ProductionStrategiesTests : IClassFixture<TestData>
    {
        private readonly TestData _testData;
        private readonly GlnService _glnService;

        public ProductionStrategiesTests(TestData testData)
        {
            _testData = testData;
            _glnService = new GlnService("datahub_gln", "esett_gln");
        }

        [Fact]
        public void ProductionStrategyPerGaBrpEs_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var hourlyProductionHandler = new Step11ProductionStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, _glnService);
            var list = _testData.ProductionGaBrpEs;
            var beginTime = InstantPattern.General.Parse("2020-10-02T11:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T12:00:00Z").GetValueOrThrow();

            // Act
            var messages = hourlyProductionHandler.PrepareMessages(list, "D03", beginTime, endTime)
                .OrderBy(e => ((AggregationResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((AggregationResultMessage)e).BalanceResponsiblePartyMarketParticipantmRID)
                .ThenBy(e => ((AggregationResultMessage)e).BalanceSupplierPartyMarketParticipantmRID)
                .ThenBy(e => ((AggregationResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();
            var resultMsgOne = (AggregationResultMessage)messages[0];
            var resultMsgTwo = (AggregationResultMessage)messages[1];

            // Assert
            messages.Should().HaveCount(10);

            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("8520000000029");
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().Be("8510000000020");
            resultMsgOne.ReceiverMarketParticipantmRID.Should().Be("8510000000020");
            resultMsgOne.EnergyObservation.Count().Should().Be(2);

            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("8520000000029");
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().Be("8510000000020");
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("8520000000029");
            resultMsgTwo.EnergyObservation.Count().Should().Be(2);
        }

        [Fact]
        public void ProductionStrategyPerGaBrp_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var hourlyProductionHandler = new Step15ProductionPerBrpStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, _glnService);
            var list = _testData.ProductionGaBrp;
            var beginTime = InstantPattern.General.Parse("2020-10-02T11:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T12:00:00Z").GetValueOrThrow();

            // Act
            var messages = hourlyProductionHandler.PrepareMessages(list, "D03", beginTime, endTime)
                .OrderBy(e => ((AggregationResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((AggregationResultMessage)e).BalanceResponsiblePartyMarketParticipantmRID)
                .ToList();
            var resultMsgOne = (AggregationResultMessage)messages[0];
            var resultMsgTwo = (AggregationResultMessage)messages[1];

            // Assert
            messages.Should().HaveCount(7);

            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("5790000711314");
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgOne.ReceiverMarketParticipantmRID.Should().Be("5790000711314");
            resultMsgOne.EnergyObservation.Count().Should().Be(1);

            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("5790001108212");
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("5790001108212");
            resultMsgTwo.EnergyObservation.Count().Should().Be(1);
        }

        [Fact]
        public void ProductionStrategyPerGaEs_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var distListServiceSub = Substitute.For<IDistributionListService>();
            distListServiceSub.GetDistributionItem(Arg.Any<string>()).Returns("12345");
            var sut = new Step12ProductionPerSupplierStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, _glnService, distListServiceSub);
            var list = _testData.ProductionGaEs;
            var beginTime = InstantPattern.General.Parse("2020-10-02T11:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T12:00:00Z").GetValueOrThrow();

            // Act
            var messages = sut.PrepareMessages(list, "D03", beginTime, endTime)
                .OrderBy(e => ((AggregationResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((AggregationResultMessage)e).BalanceResponsiblePartyMarketParticipantmRID)
                .ThenBy(e => ((AggregationResultMessage)e).BalanceSupplierPartyMarketParticipantmRID)
                .ThenBy(e => ((AggregationResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();
            var resultMsgOne = (AggregationResultMessage)messages[0];
            var resultMsgTwo = (AggregationResultMessage)messages[1];

            // Assert
            messages.Should().HaveCount(9);

            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().Be("5790000711314");
            resultMsgOne.SenderMarketParticipantmRID.Should().Be("12345");
            resultMsgOne.ReceiverMarketParticipantmRID.Should().Be(_glnService.EsettGln);
            resultMsgOne.EnergyObservation.Count().Should().Be(1);

            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().Be("5790001108212");
            resultMsgTwo.SenderMarketParticipantmRID.Should().Be("12345");
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be(_glnService.EsettGln);
            resultMsgTwo.EnergyObservation.Count().Should().Be(1);
        }

        [Fact]
        public void ProductionStrategyPerGa_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var distListServiceSub = Substitute.For<IDistributionListService>();
            distListServiceSub.GetDistributionItem(Arg.Any<string>()).Returns("12345");
            var sut = new Step18ProductionPerGridAreaStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null,  _glnService, distListServiceSub);
            var list = _testData.ProductionGa;
            var beginTime = InstantPattern.General.Parse("2020-10-02T11:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T12:00:00Z").GetValueOrThrow();

            // Act
            var messages = sut.PrepareMessages(list, "D03", beginTime, endTime)
                .OrderBy(e => ((AggregationResultMessage)e).MeteringGridAreaDomainmRID)
                .ToList();
            var resultMsgOne = (AggregationResultMessage)messages[0];
            var resultMsgTwo = (AggregationResultMessage)messages[1];
            var resultMsgThree = (AggregationResultMessage)messages[2];

            // Assert
            messages.Should().HaveCount(3);

            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgOne.ReceiverMarketParticipantmRID.Should().Be("12345");
            resultMsgOne.EnergyObservation.Count().Should().Be(1);

            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("501");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("12345");
            resultMsgTwo.EnergyObservation.Count().Should().Be(1);

            resultMsgThree.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Production);
            resultMsgThree.MeteringGridAreaDomainmRID.Should().Be("502");
            resultMsgThree.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgThree.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgThree.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgThree.ReceiverMarketParticipantmRID.Should().Be("12345");
            resultMsgThree.EnergyObservation.Count().Should().Be(1);
        }
    }
}
