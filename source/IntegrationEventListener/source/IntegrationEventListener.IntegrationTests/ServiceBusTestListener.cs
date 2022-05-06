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
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;

namespace Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests
{
    public class ServiceBusTestListener : IAsyncDisposable
    {
        private readonly ServiceBusListenerMock _serviceBusListenerMock;

        public ServiceBusTestListener(ServiceBusListenerMock serviceBusListenerMock)
        {
            _serviceBusListenerMock = serviceBusListenerMock;
        }

        public async Task<ServiceBusMessageTestResult> ListenForMessageAsync(string correlationId)
        {
            var result = new ServiceBusMessageTestResult();
            result.IsMessageReceivedEvent = await _serviceBusListenerMock
                .When(request =>
                    request.CorrelationId == correlationId)
                .VerifyOnceAsync(receivedMessage =>
                {
                    result.Body = receivedMessage.Body;
                    result.CorrelationId = receivedMessage.CorrelationId;
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
            return result;
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusListenerMock.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}
