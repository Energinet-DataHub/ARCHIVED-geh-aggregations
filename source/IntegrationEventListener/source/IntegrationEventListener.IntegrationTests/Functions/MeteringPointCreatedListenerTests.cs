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
    [Collection(nameof(AggregationsFunctionAppCollectionFixture))]
    public class MeteringPointCreatedIntegrationTests : FunctionAppTestBase<AggregationsFunctionAppFixture>, IAsyncLifetime
    {
        private static readonly Random MyRandom = new Random();

        public MeteringPointCreatedIntegrationTests(AggregationsFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
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

        [Fact]
        public async Task When_MeteringPointCreatedEventReceived_Then_MeteringPointPeriodIsStored()
        {
            // Arrange
            var meteringPointId = RandomString(20);
            var effectiveDate = SystemClock.Instance.GetCurrentInstant();
            var message = TestMessages.CreateMpCreatedMessage(meteringPointId, effectiveDate.ToDateTimeUtc());

            // Act
            await Fixture.MPCreatedTopic.SenderClient.SendMessageAsync(message)
                .ConfigureAwait(false);

            // Assert
            await FunctionAsserts.AssertHasExecutedAsync(
                Fixture.HostManager, nameof(MeteringPointCreatedListener)).ConfigureAwait(false);
            var mps = await Fixture.MeteringPointRepository.GetByIdAndDateAsync(meteringPointId, effectiveDate).ConfigureAwait(false);

            Assert.NotNull(mps);
            Assert.Single(mps);
            var mp = mps.Single();

            Assert.Equal(Product.EnergyActive, mp.Product);
            Assert.Equal(ConnectionState.New, mp.ConnectionState);
            Assert.Equal(meteringPointId, mp.MeteringPointId);
            Assert.Equal(MeteringMethod.Physical, mp.MeteringMethod);
            Assert.Equal(SettlementMethod.Flex, mp.SettlementMethod);
            Assert.Equal(Unit.Kwh, mp.Unit);
            Assert.Equal("500", mp.GridArea);
            Assert.Equal(Resolution.Hourly, mp.Resolution);
            Assert.Equal(Instant.MaxValue.ToIso8601GeneralString(), mp.ToDate.ToIso8601GeneralString());
            Assert.Equal(MeteringPointType.Consumption, mp.MeteringPointType);
            Assert.Equal(effectiveDate.ToIso8601GeneralString(), mp.FromDate.ToIso8601GeneralString());

            Fixture.HostManager.ClearHostLog();
        }

        [Fact]
        public async Task Create_Meteringpoint_Then_Connect_Then_Ensure_Store_Data_Is_Correct()
        {
            // Arrange
            var meteringPointId = RandomString(20);
            var creatingEffectiveDate = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));
            var connectingEffectiveDate = creatingEffectiveDate.Plus(Duration.FromHours(1));
            var message = TestMessages.CreateMpCreatedMessage(meteringPointId, creatingEffectiveDate.ToDateTimeUtc());
            var conMessage = TestMessages.CreateMpConnectedMessage(meteringPointId, connectingEffectiveDate.ToDateTimeUtc());

            //Act
            await Fixture.MPCreatedTopic.SenderClient.SendMessageAsync(message)
                .ConfigureAwait(false);

            //TODO when concurrency issue has been addressed remove this
            Thread.Sleep(500);
            await Fixture.MPConnectedTopic.SenderClient.SendMessageAsync(conMessage)
                .ConfigureAwait(false);

            await FunctionAsserts.AssertHasExecutedAsync(
                Fixture.HostManager, nameof(MeteringPointCreatedListener)).ConfigureAwait(false);

            await FunctionAsserts.AssertHasExecutedAsync(
                Fixture.HostManager, nameof(MeteringPointConnectedListener)).ConfigureAwait(false);

            var mps = await Fixture.MeteringPointRepository.GetByIdAndDateAsync(meteringPointId, creatingEffectiveDate).ConfigureAwait(false);

            Assert.NotNull(mps);
            Assert.Equal(2, mps.Count);

            var mp = mps.First();

            Assert.Equal(Product.EnergyActive, mp.Product);
            Assert.Equal(ConnectionState.New, mp.ConnectionState);
            Assert.Equal(meteringPointId, mp.MeteringPointId);
            Assert.Equal(MeteringMethod.Physical, mp.MeteringMethod);
            Assert.Equal(SettlementMethod.Flex, mp.SettlementMethod);
            Assert.Equal(Unit.Kwh, mp.Unit);
            Assert.Equal("500", mp.GridArea);
            Assert.Equal(Resolution.Hourly, mp.Resolution);
            Assert.Equal(MeteringPointType.Consumption, mp.MeteringPointType);
            Assert.Equal(creatingEffectiveDate.ToIso8601GeneralString(), mp.FromDate.ToIso8601GeneralString());
            Assert.Equal(connectingEffectiveDate.ToIso8601GeneralString(), mp.ToDate.ToIso8601GeneralString());

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
