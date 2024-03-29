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
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.DTOs.MeteringPoints;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.Mutators;
using Energinet.DataHub.Aggregations.Common;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Aggregations.Infrastructure.Messaging;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Aggregations.MeteringPoints
{
    public class MeteringPointConnectedListener
    {
        private const string FunctionName = nameof(MeteringPointConnectedListener);
        private readonly IEventToMasterDataTransformer<MeteringPointConnectedMutator> _eventToMasterDataTransformer;
        private readonly MessageExtractor<MeteringPointConnected> _messageExtractor;
        private readonly EventDataHelper _eventDataHelper;
        private readonly ILogger<MeteringPointConnectedListener> _logger;

        public MeteringPointConnectedListener(
            MessageExtractor<MeteringPointConnected> messageExtractor,
            EventDataHelper eventDataHelper,
            IEventToMasterDataTransformer<MeteringPointConnectedMutator> eventToMasterDataTransformer,
            ILogger<MeteringPointConnectedListener> logger)
        {
            _eventToMasterDataTransformer = eventToMasterDataTransformer;
            _messageExtractor = messageExtractor;
            _eventDataHelper = eventDataHelper;
            _logger = logger;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.MeteringPointConnectedTopicName + "%",
                "%" + EnvironmentSettingNames.MeteringPointConnectedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.IntegrationEventListenerConnectionString)] byte[] data,
            FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var meteringPointConnectedEvent = await _messageExtractor.ExtractAsync<MeteringPointConnectedEvent>(data).ConfigureAwait(false);
            await _eventToMasterDataTransformer
                .HandleTransformAsync(new MeteringPointConnectedMutator(meteringPointConnectedEvent))
                .ConfigureAwait(false);
        }
    }
}
