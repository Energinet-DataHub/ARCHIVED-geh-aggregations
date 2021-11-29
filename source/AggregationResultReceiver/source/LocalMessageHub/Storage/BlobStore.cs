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
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace Energinet.DataHub.Aggregations.LocalMessageHub.Storage
{
    public class BlobStore : IFileStore
    {
        private readonly IAggregationsBlobServiceClient _aggregationsBlobServiceClient;
        private readonly IMessageHubBlobServiceClient _messageHubBlobServiceClient;
        private readonly FileStorageConfiguration _fileStorageConfiguration;

        public BlobStore(
            IAggregationsBlobServiceClient aggregationsBlobServiceClient,
            IMessageHubBlobServiceClient messageHubBlobServiceClient,
            FileStorageConfiguration fileStorageConfiguration)
        {
            _aggregationsBlobServiceClient = aggregationsBlobServiceClient;
            _messageHubBlobServiceClient = messageHubBlobServiceClient;
            _fileStorageConfiguration = fileStorageConfiguration;
        }

        public async Task<Uri> CopyBlobAsync(string fileName)
        {
            var sourceContainerClient = _aggregationsBlobServiceClient.Client.GetBlobContainerClient(_fileStorageConfiguration.ConvertedMessagesFileStoreContainerName);
            var sourceBlob = sourceContainerClient.GetBlobClient(fileName);

            if (!await sourceBlob.ExistsAsync().ConfigureAwait(false))
                throw new InvalidOperationException();

            var destContainerClient = _messageHubBlobServiceClient.Client.GetBlobContainerClient(_fileStorageConfiguration.MessageHubFileStoreContainerName);

            var destBlob = destContainerClient.GetBlobClient(sourceBlob.Name);

            var uri = GetSharedAccessSignatureUri(sourceBlob);

            await destBlob.StartCopyFromUriAsync(uri).ConfigureAwait(false);

            return destBlob.Uri;
        }

        private static Uri GetSharedAccessSignatureUri(BlobClient client)
        {
            var expiresOn = DateTimeOffset.UtcNow.AddSeconds(30);

            return client.GenerateSasUri(BlobSasPermissions.All, expiresOn);
        }
    }
}
