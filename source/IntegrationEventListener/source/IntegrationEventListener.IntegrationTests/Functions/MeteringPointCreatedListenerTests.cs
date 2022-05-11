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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.Extensions;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Infrastructure.Mappers;
using Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Assets;
using Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Common;
using Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Fixtures;
using Energinet.DataHub.Aggregations.MeteringPoints;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using FluentAssertions;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Functions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "This is a test")]
    [Collection(nameof(AggregationsFunctionAppCollectionFixture))]
    public class MeteringPointCreatedIntegrationTests : FunctionAppTestBase<IntegrationEventListenerFunctionAppFixture>, IAsyncLifetime
    {
        private static readonly Random MyRandom = new Random();

        public MeteringPointCreatedIntegrationTests(IntegrationEventListenerFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        [Theory]
        [InlineData(MeteringPointCreated.Types.MeteringPointType.MptConsumption)]
        [InlineData(MeteringPointCreated.Types.MeteringPointType.MptProduction)]
        [InlineData(MeteringPointCreated.Types.MeteringPointType.MptExchange)]
        public async Task When_MeteringPointCreatedEventReceived_Then_MeteringPointPeriodIsStored(MeteringPointCreated.Types.MeteringPointType meteringPointType)
        {
            // Arrange
            var meteringPointId = RandomString(20);
            var effectiveDate = SystemClock.Instance.GetCurrentInstant();
            var message = TestMessages.CreateMpCreatedMessage(meteringPointId, effectiveDate.ToDateTimeUtc(), meteringPointType);

            // Act
            await Fixture.MPCreatedTopic.SenderClient.SendMessageAsync(message);

            // Assert
            await FunctionAsserts.AssertHasExecutedAsync(
                Fixture.HostManager, nameof(MeteringPointCreatedListener));
            var meteringPointPeriods = await Fixture.MeteringPointRepository.GetByIdAndDateAsync(meteringPointId, effectiveDate);

            Assert.NotNull(meteringPointPeriods);
            Assert.Single(meteringPointPeriods);
            var mp = meteringPointPeriods.Single();

            Assert.Equal(Product.EnergyActive, mp.Product);
            Assert.Equal(ConnectionState.New, mp.ConnectionState);
            Assert.Equal(meteringPointId, mp.MeteringPointId);
            Assert.Equal(MeteringMethod.Physical, mp.MeteringMethod);
            Assert.Equal(SettlementMethod.Flex, mp.SettlementMethod);
            Assert.Equal(Unit.Kwh, mp.Unit);
            Assert.Equal("500", mp.GridArea);
            Assert.Equal(Resolution.Hourly, mp.Resolution);
            Assert.Equal(Instant.MaxValue.ToIso8601GeneralString(), mp.ToDate.ToIso8601GeneralString());
            Assert.Equal(effectiveDate.ToIso8601GeneralString(), mp.FromDate.ToIso8601GeneralString());
            Assert.Equal(ProtobufToDomainTypeMapper.MapMeteringPointType(meteringPointType), mp.MeteringPointType);
            Fixture.HostManager.ClearHostLog();
        }

        [Fact]
        public async Task Create_Meteringpoint_Then_Connect_Then_Ensure_Store_Data_Is_Correct()
        {
            // Arrange
            var meteringPointId = RandomString(20);
            var creatingEffectiveDate = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));
            var connectingEffectiveDate = creatingEffectiveDate.Plus(Duration.FromHours(1));
            var meteringPointCreatedMessage = TestMessages.CreateMpCreatedMessage(
                meteringPointId,
                creatingEffectiveDate.ToDateTimeUtc(),
                MeteringPointCreated.Types.MeteringPointType.MptConsumption);
            var meteringPointConnectedMessage = TestMessages.CreateMpConnectedMessage(meteringPointId, connectingEffectiveDate.ToDateTimeUtc());

            //Act
            await Fixture.MPCreatedTopic.SenderClient.SendMessageAsync(meteringPointCreatedMessage);

            await FunctionAsserts.AssertHasExecutedAsync(
                Fixture.HostManager, nameof(MeteringPointCreatedListener));

            //TODO when concurrency issue has been addressed remove this
            Thread.Sleep(500);
            await Fixture.MPConnectedTopic.SenderClient.SendMessageAsync(meteringPointConnectedMessage);

            await FunctionAsserts.AssertHasExecutedAsync(
                Fixture.HostManager, nameof(MeteringPointConnectedListener));

            var meteringPointPeriods = await Fixture.MeteringPointRepository.GetByIdAndDateAsync(meteringPointId, creatingEffectiveDate);

            Assert.NotNull(meteringPointPeriods);
            //We should now have to meteringpoint periods
            Assert.Equal(2, meteringPointPeriods.Count);

            //Assert properties on the first period
            var meteringPointPeriodOne = meteringPointPeriods.First();

            Assert.Equal(Product.EnergyActive, meteringPointPeriodOne.Product);
            Assert.Equal(ConnectionState.New, meteringPointPeriodOne.ConnectionState);
            Assert.Equal(meteringPointId, meteringPointPeriodOne.MeteringPointId);
            Assert.Equal(MeteringMethod.Physical, meteringPointPeriodOne.MeteringMethod);
            Assert.Equal(SettlementMethod.Flex, meteringPointPeriodOne.SettlementMethod);
            Assert.Equal(Unit.Kwh, meteringPointPeriodOne.Unit);
            Assert.Equal("500", meteringPointPeriodOne.GridArea);
            Assert.Equal(Resolution.Hourly, meteringPointPeriodOne.Resolution);
            Assert.Equal(MeteringPointType.Consumption, meteringPointPeriodOne.MeteringPointType);
            Assert.Equal(creatingEffectiveDate.ToIso8601GeneralString(), meteringPointPeriodOne.FromDate.ToIso8601GeneralString());
            Assert.Equal(connectingEffectiveDate.ToIso8601GeneralString(), meteringPointPeriodOne.ToDate.ToIso8601GeneralString());

            //Assert properties on the second period where the metering point has been connected
            var meteringPointPeriodTwo = meteringPointPeriods.ToArray()[1];

            Assert.Equal(Product.EnergyActive, meteringPointPeriodTwo.Product);
            Assert.Equal(ConnectionState.Connected, meteringPointPeriodTwo.ConnectionState);
            Assert.Equal(meteringPointId, meteringPointPeriodTwo.MeteringPointId);
            Assert.Equal(MeteringMethod.Physical, meteringPointPeriodTwo.MeteringMethod);
            Assert.Equal(SettlementMethod.Flex, meteringPointPeriodTwo.SettlementMethod);
            Assert.Equal(Unit.Kwh, meteringPointPeriodTwo.Unit);
            Assert.Equal("500", meteringPointPeriodTwo.GridArea);
            Assert.Equal(Resolution.Hourly, meteringPointPeriodTwo.Resolution);
            Assert.Equal(MeteringPointType.Consumption, meteringPointPeriodTwo.MeteringPointType);
            Assert.Equal(connectingEffectiveDate.ToIso8601GeneralString(), meteringPointPeriodTwo.FromDate.ToIso8601GeneralString());
            Assert.Equal(Instant.MaxValue.ToIso8601GeneralString(), meteringPointPeriodTwo.ToDate.ToIso8601GeneralString());

            Fixture.HostManager.ClearHostLog();
        }

        private static string RandomString(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[MyRandom.Next(s.Length)]).ToArray());
        }
    }
}
