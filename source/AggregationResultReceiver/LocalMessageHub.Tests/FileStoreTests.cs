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
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.LocalMessageHub.Tests
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public sealed class FileStoreTests
    {
        [Fact]
        public async Task CopyBlobAsync_CopyDataFromOneBlobStorageToAnother_Success()
        {
            // Arrange
            const string filename = "Charges(Master).csv";

            IAggregationsBlobServiceClient aggregationsBlobStore = new BlobServiceClientProvider(new BlobServiceClient("UseDevelopmentStorage=true"));
            IMessageHubBlobServiceClient messageHubBlobStore = new BlobServiceClientProvider(new BlobServiceClient("UseDevelopmentStorage=true"));

            var blobStorage = new BlobServiceClient("UseDevelopmentStorage=true");

            var sourceContainer = blobStorage.GetBlobContainerClient("test-data-source");

            if (!await sourceContainer.ExistsAsync().ConfigureAwait(false))
            {
                await sourceContainer.CreateIfNotExistsAsync().ConfigureAwait(false);
            }

            var destContainer = blobStorage.GetBlobContainerClient("test-blobstorage");

            if (!await destContainer.ExistsAsync().ConfigureAwait(false))
            {
                await destContainer.CreateIfNotExistsAsync().ConfigureAwait(false);
            }

            if (!await sourceContainer.DeleteBlobIfExistsAsync(filename).ConfigureAwait(false))
            {
                await blobStorage.GetBlobContainerClient("test-data-source").UploadBlobAsync(filename, new MemoryStream())
                    .ConfigureAwait(false);
            }

            var fileStore = new BlobStore(
                aggregationsBlobStore,
                messageHubBlobStore,
                new FileStorageConfiguration("test-blobstorage", "test-data-source"));

            // Act
            var destUri = await fileStore.CopyBlobAsync(filename)
                .ConfigureAwait(false);

            // Assert
            Assert.Equal(new Uri("http://127.0.0.1:10000/devstoreaccount1/test-blobstorage/Charges%28Master%29.csv"), destUri);
        }
    }
}
