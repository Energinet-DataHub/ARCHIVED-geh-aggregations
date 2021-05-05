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
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Aggregation.Application.Coordinator.Strategies;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.ResultMessages;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Tests.Assets;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
{
    [Trait("Category", "Component")]
    public class AggregationHandlerTests : IClassFixture<TestData>
    {
        private readonly TestData _testDataProvider;

        public AggregationHandlerTests(TestData testDataProvider)
        {
            _testDataProvider = testDataProvider;
        }

        [Fact]
        public void Check_Count_Of_HourlyConsumption_Handler_Test()
        {
            var hourlyConsumptionHandler = new ConsumptionStrategy(
                Substitute.For<IGLNService>(),
                Substitute.For<ILogger<ConsumptionDto>>(),
                null);
            var list = _testDataProvider.HourlyConsumption;

            const string beginTime = "2020-10-02T01:00:00+0100";
            const string endTime = "2020-10-03T02:00:00+0100";
            var messages = hourlyConsumptionHandler.PrepareMessages(list, ProcessType.D03, beginTime, endTime);

            messages.Should().HaveCount(9);
        }

        [Fact]
        public void Check_Content_Of_HourlyConsumption_Message_Test()
        {
            var hourlyConsumptionHandler = new ConsumptionStrategy(
                Substitute.For<IGLNService>(),
                Substitute.For<ILogger<ConsumptionDto>>(),
                null);
            var list = _testDataProvider.HourlyConsumption;

            const string beginTime = "2020-10-02T03:00:00+0100";
            const string endTime = "2020-10-03T04:00:00+0100";
            const ProcessType processType = ProcessType.D03;
            var messages = hourlyConsumptionHandler.PrepareMessages(list, processType, beginTime, endTime);
            var first = (AggregatedConsumptionResultMessage)messages.First();

            first.ProcessType.Should().Be(Enum.GetName(typeof(ProcessType), processType));
            first.MeteringGridAreaDomainMRID.Should().Be("500");
            first.BalanceResponsiblePartyMarketParticipantMRID.Should().Be("8520000000005");
            first.BalanceSupplierPartyMarketParticipantMRID.Should().Be("8510000000006");
            first.TimeIntervalStart.Should().Be(beginTime);
            first.TimeIntervalEnd.Should().Be(endTime);
            first.Quantities.First().Should().Be(96);
        }

        [Fact]
        public void Check_Count_Of_FlexConsumption_Handler_Test()
        {
            var flexConsumptionHandler = new FlexConsumptionStrategy(
                Substitute.For<IGLNService>(),
                Substitute.For<ISpecialMeteringPointsService>(),
                Substitute.For<ILogger<ConsumptionDto>>(),
                null);

            var list = _testDataProvider.FlexConsumption;
            const string beginTime = "2020-10-02T05:00:00+01";
            const string endTime = "2020-10-03T06:00:00+01";
            var messages = flexConsumptionHandler.PrepareMessages(list, ProcessType.D03, beginTime, endTime);

            messages.Should().HaveCount(10);
        }

        [Fact]
        public void Check_Content_Of_FlexConsumption_Message_Test()
        {
            var flexConsumptionHandler = new FlexConsumptionStrategy(
                Substitute.For<IGLNService>(),
                Substitute.For<ISpecialMeteringPointsService>(),
                Substitute.For<ILogger<ConsumptionDto>>(),
                null);

            var list = _testDataProvider.FlexConsumption;

            const string beginTime = "2020-10-02T07:00:00+01";
            const string endTime = "2020-10-03T08:00:00+01";
            const ProcessType processType = ProcessType.D04;
            var messages = flexConsumptionHandler.PrepareMessages(list, processType, beginTime, endTime);
            var first = (AggregatedConsumptionResultMessage)messages.First();

            first.ProcessType.Should().Be(Enum.GetName(typeof(ProcessType), processType));
            first.MeteringGridAreaDomainMRID.Should().Be("500");
            first.BalanceResponsiblePartyMarketParticipantMRID.Should().Be("8520000000005");
            first.BalanceSupplierPartyMarketParticipantMRID.Should().Be("8510000000006");
            first.TimeIntervalStart.Should().Be(beginTime);
            first.TimeIntervalEnd.Should().Be(endTime);
            first.Quantities.First().Should().Be(8);
        }

        [Fact]
        public void Check_Count_Of_HourlyProduction_Handler_Test()
        {
            var hourlyProductionHandler = new ProductionStrategy(
                Substitute.For<IGLNService>(),
                Substitute.For<ISpecialMeteringPointsService>(),
                Substitute.For<ILogger<ProductionDto>>(),
                null);

            var list = _testDataProvider.HourlyProduction;
            const string beginTime = "2020-10-02T09:00:00+01";
            const string endTime = "2020-10-03T10:00:00+01";
            var messages = hourlyProductionHandler.PrepareMessages(list, ProcessType.D03, beginTime, endTime);

            messages.Should().HaveCount(10);
        }

        [Fact]
        public void Check_Content_Of_HourlyProduction_Message_Test()
        {
            var hourlyProductionHandler = new ProductionStrategy(
                Substitute.For<IGLNService>(),
                Substitute.For<ISpecialMeteringPointsService>(),
                Substitute.For<ILogger<ProductionDto>>(),
                null);

            var list = _testDataProvider.HourlyProduction;
            const string beginTime = "2020-10-02T11:00:00+01";
            const string endTime = "2020-10-03T12:00:00+01";
            const ProcessType processType = ProcessType.D04;
            var messages = hourlyProductionHandler.PrepareMessages(list, processType, beginTime, endTime);
            var first = (AggregatedProductionResultMessage)messages.First();

            first.ProcessType.Should().Be(Enum.GetName(typeof(ProcessType), processType));
            first.MeteringGridAreaDomainMRID.Should().Be("500");
            first.BalanceResponsiblePartyMarketParticipantMRID.Should().Be("8520000000005");
            first.BalanceSupplierPartyMarketParticipantMRID.Should().Be("8510000000013");
            first.TimeIntervalStart.Should().Be(beginTime);
            first.TimeIntervalEnd.Should().Be(endTime);
            first.Quantities.First().Should().Be(912);
        }

        [Fact]
        public void Check_Content_Of_Exchange_Message_Test()
        {
            // Arrange
            var testData = _testDataProvider.Exchange;
            var exchangeStrategy = new ExchangeStrategy(
                Substitute.For<ILogger<ExchangeDto>>(),
                Substitute.For<IGLNService>(),
                null);

            // Act
            var message = (AggregatedExchangeResultMessage)exchangeStrategy.PrepareMessages(
                testData,
                ProcessType.D03,
                "2020-10-03T07:00:00.000Z",
                "2020-10-03T08:00:00.000Z").First();

            // Assert
            message.Kind.ShouldBeEquivalentTo(23);
            message.MeteringGridAreaDomainMRID.ShouldBeEquivalentTo("500");
            message.TimeIntervalStart.ShouldBeEquivalentTo("2020-10-03T07:00:00.000Z");
            message.TimeIntervalEnd.ShouldBeEquivalentTo("2020-10-03T08:00:00.000Z");
            message.Result.ShouldBeEquivalentTo(-32);
        }

        [Fact]
        public void Check_Content_Of_ExchangeNeighbour_Message_Test()
        {
            // Arrange
            var testData = _testDataProvider.ExchangeNeighbour;
            var exchangeStrategy = new ExchangeNeighbourStrategy(
                Substitute.For<ILogger<ExchangeNeighbourDto>>(),
                Substitute.For<IGLNService>(),
                null);

            // Act
            var message = (AggregatedExchangeNeighbourResultMessage)exchangeStrategy.PrepareMessages(
                testData,
                ProcessType.D03,
                "2020-10-03T07:00:00.000Z",
                "2020-10-03T08:00:00.000Z").First();

            // Assert
            message.Kind.ShouldBeEquivalentTo(23);
            message.MeteringGridAreaDomainMRID.ShouldBeEquivalentTo("500");
            message.InMeteringGridAreaDomainMRID.ShouldBeEquivalentTo("500");
            message.OutMeteringGridAreaDomainMRID.ShouldBeEquivalentTo("501");
            message.TimeIntervalStart.ShouldBeEquivalentTo("2020-10-03T07:00:00.000Z");
            message.TimeIntervalEnd.ShouldBeEquivalentTo("2020-10-03T08:00:00.000Z");
            message.Result.ShouldBeEquivalentTo(-32);
        }
    }
}
