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
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.MeteringPoints;
using Energinet.DataHub.Aggregations.Application.Interfaces;
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
        private readonly IEventToMasterDataTransformer _eventToMasterDataTransformer;
        private readonly MessageExtractor<MeteringPointConnected> _messageExtractor;
        private readonly EventDataHelper _eventDataHelper;
        private readonly ILogger<MeteringPointConnectedListener> _logger;

        public MeteringPointConnectedListener(
            MessageExtractor<MeteringPointConnected> messageExtractor,
            EventDataHelper eventDataHelper,
            IEventToMasterDataTransformer eventToMasterDataTransformer,
            ILogger<MeteringPointConnectedListener> logger)
        {
            _eventToMasterDataTransformer = eventToMasterDataTransformer;
            _messageExtractor = messageExtractor;
            _eventDataHelper = eventDataHelper;
            _logger = logger;
        }

        [Function("MeteringPointConnectedListener")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%METERING_POINT_CONNECTED_TOPIC_NAME%",
                "%METERING_POINT_CONNECTED_SUBSCRIPTION_NAME%",
                Connection = "INTEGRATION_EVENT_LISTENER_CONNECTION_STRING")] byte[] data,
            FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var eventMetaData = _eventDataHelper.GetEventMetaData(context);

            _logger.LogTrace("MeteringPointConnected event received with {OperationCorrelationId}", eventMetaData.OperationCorrelationId);
            try
            {
                var meteringPointConnectedEvent = await _messageExtractor.ExtractAsync<MeteringPointConnectedEvent>(data).ConfigureAwait(false);

                _logger.LogInformation("Converted protobuf message with {MeteringPointId}", meteringPointConnectedEvent.MeteringPointId);
                await _eventToMasterDataTransformer
                    .HandleTransformAsync<MeteringPointConnectedEvent, MeteringPoint>(meteringPointConnectedEvent)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Could not handle {nameof(MeteringPointCreatedEvent)}");
            }
        }
    }
}
