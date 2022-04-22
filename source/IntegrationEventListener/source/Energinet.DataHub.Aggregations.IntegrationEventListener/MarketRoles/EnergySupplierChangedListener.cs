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
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.DTOs.MeteringPoints;
using Energinet.DataHub.Aggregations.Common;
using Energinet.DataHub.Aggregations.Infrastructure.Messaging;
using Energinet.DataHub.MarketRoles.IntegrationEventContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Aggregations.MarketRoles
{
    public class EnergySupplierChangedListener
    {
        private readonly MessageExtractor<EnergySupplierChanged> _messageExtractor;
        private readonly EventDataHelper _eventDataHelper;
        private readonly ILogger<EnergySupplierChangedListener> _logger;

        public EnergySupplierChangedListener(MessageExtractor<EnergySupplierChanged> messageExtractor,  EventDataHelper eventDataHelper, ILogger<EnergySupplierChangedListener> logger)
        {
            _messageExtractor = messageExtractor;
            _eventDataHelper = eventDataHelper;
            _logger = logger;
        }

        [Function("EnergySupplierChangedListener")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.EnergySupplierChangedTopicName + "%",
                "%" + EnvironmentSettingNames.EnergySupplierChangedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.IntegrationEventListenerConnectionString)] byte[] data,
            FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var request = await _messageExtractor.ExtractAsync<EnergySupplierChangedEvent>(data).ConfigureAwait(false);
            //TODO implement HandleTransformAsync
        }
    }
}
