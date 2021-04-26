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
using System.Text.Json;
using FluentAssertions;
using GreenEnergyHub.Aggregation.Application.GLN;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Tests.Assets;
using NSubstitute;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
{
    [Trait("Category", "Component")]
    public class AggregationHandlerTests : IClassFixture<TestData>
    {
        public AggregationHandlerTests(TestData testData)
        {
        }

        //[Fact]
        //public void Check_Count_Of_HourlyConsumption_Handler_Test()
        //{
        //    var hourlyConsumptionHandler = new HourlyConsumptionHandler(Substitute.For<IGLNService>());

        //    const string beginTime = "2020-10-02T01:00:00+0100";
        //    const string endTime = "2020-10-03T02:00:00+0100";
        //    var messages = hourlyConsumptionHandler.PrepareMessages(_results.HourlyConsumption, ProcessType.D03, beginTime, endTime);

        //    messages.Should().HaveCount(9);
        //}

        //[Fact]
        //public void Check_Content_Of_HourlyConsumption_Message_Test()
        //{
        //    var hourlyConsumptionHandler = new HourlyConsumptionHandler(Substitute.For<IGLNService>());

        //    const string beginTime = "2020-10-02T03:00:00+0100";
        //    const string endTime = "2020-10-03T04:00:00+0100";
        //    const ProcessType processType = ProcessType.D03;
        //    var messages = hourlyConsumptionHandler.PrepareMessages(_results.HourlyConsumption, processType, beginTime, endTime);
        //    var first = (AggregatedMeteredDataTimeSeries)messages.First();

        //    first.ProcessType.Should().Be(Enum.GetName(typeof(ProcessType), processType));
        //    first.MeteringGridAreaDomainMRid.Should().Be("500");
        //    first.BalanceResponsiblePartyMarketParticipantMRid.Should().Be("8520000000005");
        //    first.BalanceSupplierPartyMarketParticipantMRid.Should().Be("8510000000006");
        //    first.TimeIntervalStart.Should().Be(beginTime);
        //    first.TimeIntervalEnd.Should().Be(endTime);
        //    first.Quantities.First().Should().Be(96);
        //}

        //[Fact]
        //public void Check_Count_Of_FlexConsumption_Handler_Test()
        //{
        //    var flexConsumptionHandler = new FlexConsumptionHandler(Substitute.For<IGLNService>(), Substitute.For<ISpecialMeteringPointsService>());

        //    const string beginTime = "2020-10-02T05:00:00+01";
        //    const string endTime = "2020-10-03T06:00:00+01";
        //    var messages = flexConsumptionHandler.PrepareMessages(_results.FlexConsumption, ProcessType.D03, beginTime, endTime);

        //    messages.Should().HaveCount(10);
        //}

        //[Fact]
        //public void Check_Content_Of_FlexConsumption_Message_Test()
        //{
        //    var flexConsumptionHandler = new FlexConsumptionHandler(Substitute.For<IGLNService>(), Substitute.For<ISpecialMeteringPointsService>());

        //    const string beginTime = "2020-10-02T07:00:00+01";
        //    const string endTime = "2020-10-03T08:00:00+01";
        //    const ProcessType processType = ProcessType.D04;
        //    var messages = flexConsumptionHandler.PrepareMessages(_results.FlexConsumption, processType, beginTime, endTime);
        //    var first = (AggregatedMeteredDataTimeSeries)messages.First();

        //    first.ProcessType.Should().Be(Enum.GetName(typeof(ProcessType), processType));
        //    first.MeteringGridAreaDomainMRid.Should().Be("500");
        //    first.BalanceResponsiblePartyMarketParticipantMRid.Should().Be("8520000000005");
        //    first.BalanceSupplierPartyMarketParticipantMRid.Should().Be("8510000000006");
        //    first.TimeIntervalStart.Should().Be(beginTime);
        //    first.TimeIntervalEnd.Should().Be(endTime);
        //    first.Quantities.First().Should().Be(8);
        //}

        //[Fact]
        //public void Check_Count_Of_HourlyProduction_Handler_Test()
        //{
        //    var hourlyProductionHandler = new HourlyProductionHandler(Substitute.For<IGLNService>(), Substitute.For<ISpecialMeteringPointsService>());

        //    const string beginTime = "2020-10-02T09:00:00+01";
        //    const string endTime = "2020-10-03T10:00:00+01";
        //    var messages = hourlyProductionHandler.PrepareMessages(_results.HourlyProduction, ProcessType.D03, beginTime, endTime);

        //    messages.Should().HaveCount(10);
        //}

        //[Fact]
        //public void Check_Content_Of_HourlyProduction_Message_Test()
        //{
        //    var hourlyProductionHandler = new HourlyProductionHandler(Substitute.For<IGLNService>(), Substitute.For<ISpecialMeteringPointsService>());

        //    const string beginTime = "2020-10-02T11:00:00+01";
        //    const string endTime = "2020-10-03T12:00:00+01";
        //    const ProcessType processType = ProcessType.D04;
        //    var messages = hourlyProductionHandler.PrepareMessages(_results.HourlyProduction, processType, beginTime, endTime);
        //    var first = (AggregatedMeteredDataTimeSeries)messages.First();

        //    first.ProcessType.Should().Be(Enum.GetName(typeof(ProcessType), processType));
        //    first.MeteringGridAreaDomainMRid.Should().Be("500");
        //    first.BalanceResponsiblePartyMarketParticipantMRid.Should().Be("8520000000005");
        //    first.BalanceSupplierPartyMarketParticipantMRid.Should().Be("8510000000013");
        //    first.TimeIntervalStart.Should().Be(beginTime);
        //    first.TimeIntervalEnd.Should().Be(endTime);
        //    first.Quantities.First().Should().Be(912);
        //}
    }
}
