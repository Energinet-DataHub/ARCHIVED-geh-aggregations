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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.IntegrationTests.Fixtures;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.Assets;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.TestCommon;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.IntegrationTests.Functions
{
    [Collection(nameof(AggregationsFunctionAppCollectionFixture))]
    public class AggregationResultListenerTests : FunctionAppTestBase<AggregationsFunctionAppFixture>
    {
        public AggregationResultListenerTests(AggregationsFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
        }

        private TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);

        [Theory]
        [AutoData]
        public async Task When_ReceivingJobCompletedEvent_Then_EventIsProcessed([NotNull] TestDocuments testDocuments)
        {
            // Arrange
            var message = new ServiceBusMessage(testDocuments.JobCompletedEvent);
            var blocContainerClient = Fixture.BlobServiceClient.GetBlobContainerClient("converted-messages");

            // Act
            await Fixture.JobCompletedTopic.SenderClient.SendMessageAsync(message).ConfigureAwait(false);

            // Assert
            await Awaiter.WaitUntilConditionAsync(
                () =>
                blocContainerClient.GetBlobs().Any(), DefaultTimeout).ConfigureAwait(false);
        }
    }
}
