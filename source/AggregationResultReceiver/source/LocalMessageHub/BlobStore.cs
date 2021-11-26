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

namespace Energinet.DataHub.Aggregations.LocalMessageHub
{
    public class BlobStore : IFileStore
    {
        private readonly BlobServiceClient _aggregationsBlobServiceClient;
        private readonly BlobServiceClient _messageHubBlobServiceClient;
        private readonly FileStorageConfiguration _fileStorageConfiguration;

        public BlobStore(
            BlobServiceClient aggregationsBlobServiceClient,
            BlobServiceClient messageHubBlobServiceClient,
            FileStorageConfiguration fileStorageConfiguration)
        {
            _aggregationsBlobServiceClient = aggregationsBlobServiceClient;
            _messageHubBlobServiceClient = messageHubBlobServiceClient;
            _fileStorageConfiguration = fileStorageConfiguration;
        }

        public async Task<Uri> CopyBlobAsync(string fileName)
        {
            var sourceContainerClient = _aggregationsBlobServiceClient.GetBlobContainerClient(_fileStorageConfiguration.ConvertedMessagesFileStoreContainerName);
            var sourceBlob = sourceContainerClient.GetBlobClient(fileName);

            if (!await sourceBlob.ExistsAsync().ConfigureAwait(false))
                throw new InvalidOperationException();

            var destContainerClient = _messageHubBlobServiceClient.GetBlobContainerClient(_fileStorageConfiguration.MessageHubFileStoreContainerName);

            var destBlob = destContainerClient.GetBlobClient(sourceBlob.Name);

            await destBlob.StartCopyFromUriAsync(GetSharedAccessSignatureUri(sourceBlob)).ConfigureAwait(false);

            return destBlob.Uri;
        }

        private static Uri GetSharedAccessSignatureUri(BlobClient client)
        {
            var expiresOn = DateTimeOffset.UtcNow.AddSeconds(3);

            return client.GenerateSasUri(BlobSasPermissions.All, expiresOn);
        }
    }
}
