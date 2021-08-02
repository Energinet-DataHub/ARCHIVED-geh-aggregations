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
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Tests.Assets;
using Microsoft.Extensions.Logging;
using NodaTime.Text;
using NSubstitute;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
{
    [Trait("Category", "Component")]
    public class ConsumptionStrategiesTests : IClassFixture<TestData>
    {
        private readonly TestData _testData;
        private readonly GlnService _glnService;

        public ConsumptionStrategiesTests(TestData testData)
        {
            _testData = testData;
            _glnService = new GlnService("datahub_gln", "esett_gln");
        }

        [Fact]
        public void HourlyConsumptionStrategyPerGaBrpEs_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var sut = new Step03HourlyConsumptionStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, _glnService);
            var testData = _testData.ConsumptionGaBrpEs;
            var beginTime = InstantPattern.General.Parse("2020-10-02T03:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T04:00:00Z").GetValueOrThrow();

            // Act
            var messages = sut.PrepareMessages(testData, "D03", beginTime, endTime)
                .OrderBy(e => ((ConsumptionResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).BalanceResponsiblePartyMarketParticipantmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).BalanceSupplierPartyMarketParticipantmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();
            var resultMsgOne = (ConsumptionResultMessage)messages[0];
            var resultMsgTwo = (ConsumptionResultMessage)messages[1];

            // Assert
            messages.Should().HaveCount(10);
            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("8520000000005");
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().Be("8510000000004");
            resultMsgOne.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgOne.ReceiverMarketParticipantmRID.Should().Be("8510000000004");
            resultMsgOne.EnergyObservation.Count().Should().Be(2);
            resultMsgOne.SettlementMethod.Should().Be(SettlementMethodType.NonProfiled);
            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("8520000000005");
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().Be("8510000000004");
            resultMsgTwo.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("8520000000005");
            resultMsgTwo.EnergyObservation.Count().Should().Be(2);
            resultMsgTwo.SettlementMethod.Should().Be(SettlementMethodType.NonProfiled);
        }

        [Fact]
        public void HourlyConsumptionStrategyPerGaBrp_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var sut = new Step16HourlyConsumptionPerBrpStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null,  _glnService);
            var testData = _testData.ConsumptionGaBrp;
            var beginTime = InstantPattern.General.Parse("2020-10-02T03:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T04:00:00Z").GetValueOrThrow();

            // Act
            var messages = sut.PrepareMessages(testData, "D03", beginTime, endTime)
                .OrderBy(e => ((ConsumptionResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).BalanceResponsiblePartyMarketParticipantmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();
            var resultMsgOne = (ConsumptionResultMessage)messages[0];
            var resultMsgTwo = (ConsumptionResultMessage)messages[1];

            // Assert
            messages.Should().HaveCount(7);
            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("5790000711314");
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgOne.ReceiverMarketParticipantmRID.Should().Be("5790000711314");
            resultMsgOne.EnergyObservation.Count().Should().Be(1);
            resultMsgOne.SettlementMethod.Should().Be(SettlementMethodType.NonProfiled);
            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("7080005010788");
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("7080005010788");
            resultMsgTwo.EnergyObservation.Count().Should().Be(1);
            resultMsgTwo.SettlementMethod.Should().Be(SettlementMethodType.NonProfiled);
        }

        [Fact]
        public void HourlyConsumptionStrategyPerGaEs_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var distListServiceSub = Substitute.For<IDistributionListService>();
            distListServiceSub.GetDistributionItem(Arg.Any<string>()).Returns("12345");
            var sut = new Step13HourlyConsumptionPerSupplierStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, _glnService, distListServiceSub);
            var list = _testData.ConsumptionGaEs;

            var beginTime = InstantPattern.General.Parse("2020-10-02T07:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T08:00:00Z").GetValueOrThrow();

            // Act
            var messages = sut.PrepareMessages(list, "D03", beginTime, endTime)
                .OrderBy(e => ((ConsumptionResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).BalanceSupplierPartyMarketParticipantmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();
            var resultMsgOne = (ConsumptionResultMessage)messages[0];
            var resultMsgTwo = (ConsumptionResultMessage)messages[1];

            // Assert
            messages.Should().HaveCount(8);
            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().Be("7080005010788");
            resultMsgTwo.SenderMarketParticipantmRID.Should().Be("12345");
            resultMsgOne.ReceiverMarketParticipantmRID.Should().Be(_glnService.EsettGln);
            resultMsgOne.EnergyObservation.Count().Should().Be(1);
            resultMsgOne.SettlementMethod.Should().Be(SettlementMethodType.NonProfiled);
            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().Be("7609999121203");
            resultMsgTwo.SenderMarketParticipantmRID.Should().Be("12345");
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be(_glnService.EsettGln);
            resultMsgTwo.EnergyObservation.Count().Should().Be(2);
            resultMsgTwo.SettlementMethod.Should().Be(SettlementMethodType.NonProfiled);
        }

        [Fact]
        public void HourlyConsumptionStrategyPerGa_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var distListServiceSub = Substitute.For<IDistributionListService>();
            distListServiceSub.GetDistributionItem(Arg.Any<string>()).Returns("12345");
            var sut = new Step19HourlyConsumptionPerGridAreaStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, _glnService, distListServiceSub);
            var testData = _testData.ConsumptionGa;
            var beginTime = InstantPattern.General.Parse("2020-10-02T03:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T04:00:00Z").GetValueOrThrow();

            // Act
            var messages = sut.PrepareMessages(testData, "D03", beginTime, endTime)
                .OrderBy(e => ((ConsumptionResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();
            var resultMsgOne = (ConsumptionResultMessage)messages[0];
            var resultMsgTwo = (ConsumptionResultMessage)messages[1];
            var resultMsgThree = (ConsumptionResultMessage)messages[2];

            // Assert
            messages.Should().HaveCount(3);
            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("12345");
            resultMsgOne.EnergyObservation.Count().Should().Be(2);
            resultMsgOne.SettlementMethod.Should().Be(SettlementMethodType.NonProfiled);

            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("501");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("12345");
            resultMsgTwo.EnergyObservation.Count().Should().Be(2);
            resultMsgTwo.SettlementMethod.Should().Be(SettlementMethodType.NonProfiled);

            resultMsgThree.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgThree.MeteringGridAreaDomainmRID.Should().Be("502");
            resultMsgThree.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgThree.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgThree.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgThree.ReceiverMarketParticipantmRID.Should().Be("12345");
            resultMsgThree.EnergyObservation.Count().Should().Be(2);
            resultMsgThree.SettlementMethod.Should().Be(SettlementMethodType.NonProfiled);
        }

        [Fact]
        public void FlexConsumptionStrategyPerGaBrpEs_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var sut = new Step10FlexConsumptionStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, _glnService);
            var list = _testData.FlexConsumptionGaBrpEs;

            var beginTime = InstantPattern.General.Parse("2020-10-02T07:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T08:00:00Z").GetValueOrThrow();

            // Act
            var messages = sut.PrepareMessages(list, "D03", beginTime, endTime)
                .OrderBy(e => ((ConsumptionResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).BalanceResponsiblePartyMarketParticipantmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).BalanceSupplierPartyMarketParticipantmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();

            // Assert
            var resultMsgOne = (ConsumptionResultMessage)messages[0];
            var resultMsgTwo = (ConsumptionResultMessage)messages[1];
            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("8520000000002");
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().Be("8510000000001");
            resultMsgOne.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgOne.ReceiverMarketParticipantmRID.Should().Be("8510000000001");
            resultMsgOne.EnergyObservation.Count().Should().Be(2);
            resultMsgOne.SettlementMethod.Should().Be(SettlementMethodType.FlexSettledEbix);
            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("8520000000002");
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().Be("8510000000001");
            resultMsgTwo.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("8520000000002");
            resultMsgTwo.EnergyObservation.Count().Should().Be(2);
            resultMsgTwo.SettlementMethod.Should().Be(SettlementMethodType.FlexSettledEbix);
        }

        [Fact]
        public void FlexConsumptionStrategyPerGaBrp_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var sut = new Step17FlexConsumptionPerBrpStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, _glnService);
            var list = _testData.FlexConsumptionGaBrp;

            var beginTime = InstantPattern.General.Parse("2020-10-02T07:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T08:00:00Z").GetValueOrThrow();

            // Act
            var messages = sut.PrepareMessages(list, "D03", beginTime, endTime)
                .OrderBy(e => ((ConsumptionResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).BalanceResponsiblePartyMarketParticipantmRID)
                .ToList();

            // Assert
            var resultMsgOne = (ConsumptionResultMessage)messages[0];
            var resultMsgTwo = (ConsumptionResultMessage)messages[1];
            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("5790000711314");
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgOne.ReceiverMarketParticipantmRID.Should().Be("5790000711314");
            resultMsgOne.EnergyObservation.Count().Should().Be(1);
            resultMsgOne.SettlementMethod.Should().Be(SettlementMethodType.FlexSettledEbix);
            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().Be("7080005010788");
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("7080005010788");
            resultMsgTwo.EnergyObservation.Count().Should().Be(1);
            resultMsgTwo.SettlementMethod.Should().Be(SettlementMethodType.FlexSettledEbix);
        }

        [Fact]
        public void FlexConsumptionStrategyPerGaEs_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var distListServiceSub = Substitute.For<IDistributionListService>();
            distListServiceSub.GetDistributionItem(Arg.Any<string>()).Returns("12345");
            var sut = new Step14FlexConsumptionPerSupplierStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, _glnService, distListServiceSub);
            var list = _testData.FlexConsumptionGaEs;
            var beginTime = InstantPattern.General.Parse("2020-10-02T07:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T08:00:00Z").GetValueOrThrow();

            // Act
            var messages = sut.PrepareMessages(list, "D03", beginTime, endTime)
                .OrderBy(e => ((ConsumptionResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).BalanceSupplierPartyMarketParticipantmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();
            var resultMsgOne = (ConsumptionResultMessage)messages[0];
            var resultMsgTwo = (ConsumptionResultMessage)messages[1];

            // Assert
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().Be("5790000711314");
            resultMsgOne.SenderMarketParticipantmRID.Should().Be("12345");
            resultMsgOne.ReceiverMarketParticipantmRID.Should().Be(_glnService.EsettGln);
            resultMsgOne.EnergyObservation.Count().Should().Be(2);
            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgOne.SettlementMethod.Should().Be(SettlementMethodType.FlexSettledNbs);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().Be("7080005010788");
            resultMsgOne.SenderMarketParticipantmRID.Should().Be("12345");
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be(_glnService.EsettGln);
            resultMsgTwo.EnergyObservation.Count().Should().Be(1);
            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgTwo.SettlementMethod.Should().Be(SettlementMethodType.FlexSettledNbs);
        }

        [Fact]
        public void FlexConsumptionStrategyPerGa_PrepareMessages_CorrectGranulationAndContent()
        {
            // Arrange
            var distListServiceSub = Substitute.For<IDistributionListService>();
            distListServiceSub.GetDistributionItem(Arg.Any<string>()).Returns("12345");
            var sut = new Step20FlexConsumptionPerGridAreaStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null,  _glnService, distListServiceSub);
            var testData = _testData.FlexConsumptionGa;
            var beginTime = InstantPattern.General.Parse("2020-10-02T03:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T04:00:00Z").GetValueOrThrow();

            // Act
            var messages = sut.PrepareMessages(testData, "D03", beginTime, endTime)
                .OrderBy(e => ((ConsumptionResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();
            var resultMsgOne = (ConsumptionResultMessage)messages[0];
            var resultMsgTwo = (ConsumptionResultMessage)messages[1];
            var resultMsgThree = (ConsumptionResultMessage)messages[2];

            // Assert
            messages.Should().HaveCount(3);
            resultMsgOne.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgOne.MeteringGridAreaDomainmRID.Should().Be("500");
            resultMsgOne.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgOne.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("12345");
            resultMsgOne.EnergyObservation.Count().Should().Be(1);
            resultMsgOne.SettlementMethod.Should().Be(SettlementMethodType.FlexSettledEbix);

            resultMsgTwo.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgTwo.MeteringGridAreaDomainmRID.Should().Be("501");
            resultMsgTwo.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgTwo.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgTwo.ReceiverMarketParticipantmRID.Should().Be("12345");
            resultMsgTwo.EnergyObservation.Count().Should().Be(1);
            resultMsgTwo.SettlementMethod.Should().Be(SettlementMethodType.FlexSettledEbix);

            resultMsgThree.MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            resultMsgThree.MeteringGridAreaDomainmRID.Should().Be("502");
            resultMsgThree.BalanceResponsiblePartyMarketParticipantmRID.Should().BeNull();
            resultMsgThree.BalanceSupplierPartyMarketParticipantmRID.Should().BeNull();
            resultMsgThree.SenderMarketParticipantmRID.Should().Be(_glnService.DataHubGln);
            resultMsgThree.ReceiverMarketParticipantmRID.Should().Be("12345");
            resultMsgThree.EnergyObservation.Count().Should().Be(1);
            resultMsgThree.SettlementMethod.Should().Be(SettlementMethodType.FlexSettledEbix);
        }
    }
}
