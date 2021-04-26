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
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Infrastructure.BlobStorage
{
    public class BlobService : IBlobService
    {
        private readonly ILogger<BlobService> _logger;
        private readonly BlobContainerClient _blobContainerClient;

        public BlobService(CoordinatorSettings coordinatorSettings, ILogger<BlobService> logger)
        {
            _logger = logger;
            try
            {
                var blobServiceClient =
                    new BlobServiceClient(
                        $"DefaultEndpointsProtocol=https;AccountName={coordinatorSettings.InputStorageAccountName};AccountKey={coordinatorSettings.InputStorageAccountKey};EndpointSuffix=core.windows.net");
                _blobContainerClient =
                    blobServiceClient.GetBlobContainerClient(coordinatorSettings.InputStorageContainerName);
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
                var client = _blobContainerClient.GetBlobClient(inputPath);
                var stream = await client.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                // return a decompressed stream
                return new GZipStream(stream, CompressionMode.Decompress);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Could not GetBlobStreamAsync");
                throw;
            }
        }
    }
}
