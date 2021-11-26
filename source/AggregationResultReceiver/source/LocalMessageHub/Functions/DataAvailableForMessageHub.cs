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
using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Aggregations.LocalMessageHub.Functions
{
    public class DataAvailableForMessageHub
    {
        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;

        public DataAvailableForMessageHub(IDataAvailableNotificationSender dataAvailableNotificationSender)
        {
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
        }

        [Function("DataAvailableForMessageHub")]
        public async Task RunAsync(
            [BlobTrigger("data-ready-cimxml/{name}", Connection = "ConvertedMessagesBlobStorageConnectionString")]
            string myblob,
            string name,
            IDictionary<string, string> metaData,
            FunctionContext context)
        {
            if (metaData is null)
            {
                throw new ArgumentNullException(nameof(metaData));
            }

            var logger = context.GetLogger("DataAvailableForMessageHub");
            logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name}");

            var correlationIdParsed = metaData.TryGetValue("correlationId", out var correlationId);
            var recipientParsed = metaData.TryGetValue("recipient", out var recipient);
            var dataUniqueIdParsed = metaData.TryGetValue("dataUniqueId", out var dataUniqueId);

            if (correlationIdParsed && recipientParsed && dataUniqueIdParsed)
            {
                var dataAvailable = BuildDataAvailableDto(recipient, Guid.Parse(dataUniqueId));
                await _dataAvailableNotificationSender.SendAsync(correlationId, dataAvailable).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException("MetaData could not be parsed or missing data");
            }
        }

        private static DataAvailableNotificationDto BuildDataAvailableDto(string recipientGln, Guid dataId)
        {
            var recipient = new GlobalLocationNumberDto(recipientGln);
            var messageType = new MessageTypeDto("Type");

            return new DataAvailableNotificationDto(dataId, recipient, messageType, DomainOrigin.Aggregations, false, 1);
        }
    }
}
