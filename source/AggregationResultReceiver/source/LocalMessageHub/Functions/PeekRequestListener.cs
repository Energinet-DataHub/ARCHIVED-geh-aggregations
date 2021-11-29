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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.LocalMessageHub.Storage;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Extensions;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Peek;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Aggregations.LocalMessageHub.Functions
{
    public class PeekRequestListener
    {
        private readonly IStorageHandler _storageHandler;
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly IDataBundleResponseSender _dataBundleResponseSender;
        private IFileStore _fileStore;

        public PeekRequestListener(
            IRequestBundleParser requestBundleParser,
            IDataBundleResponseSender dataBundleResponseSender,
            IFileStore fileStore,
            IStorageHandler storageHandler)
        {
            _requestBundleParser = requestBundleParser;
            _dataBundleResponseSender = dataBundleResponseSender;
            _fileStore = fileStore;
            _storageHandler = storageHandler;
        }

        [Function("PeekRequestListener")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%PeekListenerQueueName%",
                Connection = "ServiceBusConnectionString",
                IsSessionsEnabled = true)]
            byte[] request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var message = _requestBundleParser.Parse(request);

            var dataAvailableIds = await _storageHandler.GetDataAvailableNotificationIdsAsync(message).ConfigureAwait(false);

            var fileNameToCopy = dataAvailableIds[0].ToString();
            var filename = await _fileStore.CopyBlobAsync(fileNameToCopy).ConfigureAwait(false);

            if (filename is null)
            {
                throw new FileNotFoundException();
            }

            await ReplyToPeekRequestAsync(message, filename).ConfigureAwait(false);
        }

        private async Task ReplyToPeekRequestAsync(DataBundleRequestDto request, Uri filepath)
        {
            var replyMessage = request.CreateResponse(filepath);

            await _dataBundleResponseSender.SendAsync(replyMessage).ConfigureAwait(false);
        }
    }
}
