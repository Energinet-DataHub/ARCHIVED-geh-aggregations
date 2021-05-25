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
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Tests.Assets;
using Microsoft.Extensions.Logging;
using NodaTime.Text;
using NSubstitute;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
{
    public class Step10FlexConsumptionStrategyTests : IClassFixture<TestData>
    {
        private readonly TestData _testData;

        public Step10FlexConsumptionStrategyTests(TestData testData)
        {
            _testData = testData;
        }

        [Fact]
        public void Check_Count_Of_FlexConsumption_Handler_Test()
        {
            // Arrange
            var flexConsumptionHandler = new Step10FlexConsumptionStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, null, Substitute.For<IGLNService>());

            var list = _testData.FlexConsumption;
            var beginTime = InstantPattern.General.Parse("2020-10-02T05:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T06:00:00Z").GetValueOrThrow();

            // Act
            var messages = flexConsumptionHandler.PrepareMessages(list, "D03", beginTime, endTime);

            // Assert
            messages.Should().HaveCount(10);
        }

        [Fact]
        public void Check_Content_Of_FlexConsumption_Message_Test()
        {
            // Arrange
            var flexConsumptionHandler = new Step10FlexConsumptionStrategy(Substitute.For<ILogger<AggregationResultDto>>(), null, Substitute.For<IJsonSerializer>(), Substitute.For<IGLNService>());
            var list = _testData.FlexConsumption;

            var beginTime = InstantPattern.General.Parse("2020-10-02T07:00:00Z").GetValueOrThrow();
            var endTime = InstantPattern.General.Parse("2020-10-03T08:00:00Z").GetValueOrThrow();
            const string processType = "D04";

            // Act
            // Act
            var messages = flexConsumptionHandler.PrepareMessages(list, "D03", beginTime, endTime)
                .OrderBy(e => ((ConsumptionResultMessage)e).MeteringGridAreaDomainmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).BalanceResponsiblePartyMarketParticipantmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).BalanceSupplierPartyMarketParticipantmRID)
                .ThenBy(e => ((ConsumptionResultMessage)e).ReceiverMarketParticipantmRID)
                .ToList();

            // Assert
            ((ConsumptionResultMessage)messages[0]).MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            ((ConsumptionResultMessage)messages[0]).MeteringGridAreaDomainmRID.Should().Be("500");
            ((ConsumptionResultMessage)messages[0]).BalanceResponsiblePartyMarketParticipantmRID.Should().Be("8520000000002");
            ((ConsumptionResultMessage)messages[0]).BalanceSupplierPartyMarketParticipantmRID.Should().Be("8510000000001");
            ((ConsumptionResultMessage)messages[0]).ReceiverMarketParticipantmRID.Should().Be("8510000000001");
            ((ConsumptionResultMessage)messages[0]).EnergyObservation.Count().Should().Be(2);
            ((ConsumptionResultMessage)messages[0]).SettlementMethod.Should().Be(SettlementMethodType.FlexSettledEbix);
            ((ConsumptionResultMessage)messages[1]).MarketEvaluationPointType.Should().Be(MarketEvaluationPointType.Consumption);
            ((ConsumptionResultMessage)messages[1]).MeteringGridAreaDomainmRID.Should().Be("500");
            ((ConsumptionResultMessage)messages[1]).BalanceResponsiblePartyMarketParticipantmRID.Should().Be("8520000000002");
            ((ConsumptionResultMessage)messages[1]).BalanceSupplierPartyMarketParticipantmRID.Should().Be("8510000000001");
            ((ConsumptionResultMessage)messages[1]).ReceiverMarketParticipantmRID.Should().Be("8520000000002");
            ((ConsumptionResultMessage)messages[1]).EnergyObservation.Count().Should().Be(2);
            ((ConsumptionResultMessage)messages[1]).SettlementMethod.Should().Be(SettlementMethodType.FlexSettledEbix);
        }
    }
}
