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
using Energinet.DataHub.MessageHub.Model.Dequeue;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LocalMessageHub.Functions
{
    public class DequeuedBundleListener
    {
        private readonly IDequeueNotificationParser _dequeueNotificationParser;

        public DequeuedBundleListener(IDequeueNotificationParser dequeueNotificationParser)
        {
            _dequeueNotificationParser = dequeueNotificationParser;
        }

        [Function("DequeuedBundleListener")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%DequeueQueueName%",
                Connection = "ServiceBusConnectionString")]
            byte[] message,
            FunctionContext context)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var logger = context.GetLogger<DequeuedBundleListener>();
            logger.LogInformation($"C# ServiceBus topic trigger function processed message in {context.FunctionDefinition.Name}");

            try
            {
                var (dataAvailableNotificationIds, globalLocationNumberDto) = _dequeueNotificationParser.Parse(message);
                Console.WriteLine($"Dequeue notification received, recipient: {globalLocationNumberDto}," +
                                  $" dataavailable notifications: {string.Join(",", dataAvailableNotificationIds)}");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error in {FunctionName}", context.FunctionDefinition.Name);
                throw;
            }
        }
    }
}
