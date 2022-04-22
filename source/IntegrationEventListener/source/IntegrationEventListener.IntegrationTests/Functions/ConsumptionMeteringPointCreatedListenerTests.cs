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
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Assets;
using Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Fixtures;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Functions
{
    [Collection(nameof(AggregationsFunctionAppCollectionFixture))]
    public class MeteringPointCreatedListenerTests_RunAsync : FunctionAppTestBase<AggregationsFunctionAppFixture>
    {
        public MeteringPointCreatedListenerTests_RunAsync(AggregationsFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
        }

        private TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);

        [Fact]
        public async Task When_ReceivingEvent_Then_EventIsProcessed()
        {
            // Arrange
            var message = TestMessages.CreateMpCreatedMessage();

            //TODO create test against DB
            //using var db = ThrowawayDatabase.FromLocalInstance("(LocalDB)\\MSSQLLocalDB");
            await Fixture.MPCreatedTopic.SenderClient.SendMessageAsync(message)
                .ConfigureAwait(false);

            // Assert
            //TODO data is stored in localDB
        }
    }
}
