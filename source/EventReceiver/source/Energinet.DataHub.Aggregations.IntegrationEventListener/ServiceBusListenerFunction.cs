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
using Energinet.DataHub.Aggregations.Application;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Aggregations
{
    public class ServiceBusListenerFunction
    {
        private readonly IEventDispatcher _dispatcher;
        private readonly ILogger<ServiceBusListenerFunction> _logger;

        public ServiceBusListenerFunction(IEventDispatcher dispatcher, ILogger<ServiceBusListenerFunction> logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;
        }

        [Function("charge-link-created")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                    "%charge-link-created-topic%",
                    "%ServiceBusSubscription%",
                    Connection = "ServiceBusConnection")]
            byte[] msgData,
            FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var contractName = context.FunctionDefinition.Name;
            _logger.LogInformation($"ServiceBusListenerFunction triggered for {contractName}");
            await _dispatcher.DispatchAsync(msgData, contractName).ConfigureAwait(false);
        }
    }
}
