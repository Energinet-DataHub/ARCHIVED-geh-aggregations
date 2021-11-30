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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Configurations;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Storage
{
    public class BlobStorage : IBlobStorage
    {
        private readonly FileStoreConfiguration _fileStoreConfiguration;

        public BlobStorage(FileStoreConfiguration fileStoreConfiguration)
        {
            _fileStoreConfiguration = fileStoreConfiguration;
        }

        public async Task UploadBlobAsync(string blobName, Stream content, CancellationToken cancellationToken = default)
        {
            var client = new BlobClient(
                _fileStoreConfiguration.BlobStorageConnectionString,
                _fileStoreConfiguration.AggregationResultsContainerName,
                blobName);
            await client.UploadAsync(content, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Stream> DownloadBlobAsync(string blobName, CancellationToken cancellationToken = default)
        {
            var client = new BlobClient(
                _fileStoreConfiguration.BlobStorageConnectionString,
                _fileStoreConfiguration.ConvertedMessagesContainerName,
                blobName);
            return (await client.DownloadAsync(cancellationToken).ConfigureAwait(false)).Value.Content;
        }
    }
}
