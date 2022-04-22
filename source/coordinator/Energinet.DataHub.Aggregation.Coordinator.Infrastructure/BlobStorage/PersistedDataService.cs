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
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator;
using Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator.Interfaces;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Aggregation.Coordinator.Infrastructure.BlobStorage
{
    public class PersistedDataService : IPersistedDataService
    {
        private readonly ILogger<PersistedDataService> _logger;
        private readonly BlobContainerClient _blobContainerClient;

        public PersistedDataService(CoordinatorSettings coordinatorSettings, ILogger<PersistedDataService> logger)
        {
            _logger = logger;
            try
            {
                if (coordinatorSettings == null)
                {
                    throw new ArgumentNullException(nameof(coordinatorSettings));
                }

                var blobServiceClient =
                    new BlobServiceClient(
                        $"DefaultEndpointsProtocol=https;AccountName={coordinatorSettings.DataStorageAccountName};AccountKey={coordinatorSettings.DataStorageAccountKey};EndpointSuffix=core.windows.net");
                _blobContainerClient =
                    blobServiceClient.GetBlobContainerClient(coordinatorSettings.DataStorageContainerName);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Could not start BlobService");
                throw;
            }
        }

        public async Task<Stream> GetBlobStreamAsync(string inputPath, CancellationToken cancellationToken)
        {
            try
            {
                var blobs = _blobContainerClient.GetBlobsAsync(prefix: inputPath, cancellationToken: cancellationToken);

                await foreach (var item in blobs)
                {
                    if (item.Name.EndsWith("json.gz", StringComparison.InvariantCulture))
                    {
                        var client = _blobContainerClient.GetBlobClient(item.Name);
                        var stream = await client.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                        // return a decompressed stream
                        return new GZipStream(stream, CompressionMode.Decompress);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Could not GetBlobStreamAsync");
                throw;
            }

            _logger.LogCritical("We did not match a blob to stream from");
            return Stream.Null;
        }
    }
}
