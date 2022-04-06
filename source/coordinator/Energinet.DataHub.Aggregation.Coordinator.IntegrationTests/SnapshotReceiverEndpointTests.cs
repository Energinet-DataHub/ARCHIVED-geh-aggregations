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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator;
using Energinet.DataHub.Aggregation.Coordinator.IntegrationTests.Fixtures;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Aggregation.Coordinator.IntegrationTests
{
    [Collection(nameof(CoordinatorFunctionAppCollectionFixture))]
    public class SnapshotReceiverEndpointTests : FunctionAppTestBase<CoordinatorFunctionAppFixture>
    {
        public SnapshotReceiverEndpointTests(CoordinatorFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
        }

        [Fact]
        public async Task When_RequestReceivedWithNoJwtToken_Then_OkResponseReturned()
        {
            using var request = await CreateHttpRequest().ConfigureAwait(false);
            var response = await Fixture.HostManager.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private async Task<HttpRequestMessage> CreateHttpRequest()
        {
            const string requestUri = "api/" + CoordinatorFunctionNames.SnapshotReceiver;
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            request.Headers.Add("snapshot-id", Guid.NewGuid().ToString());
            return request;
        }
    }
}
