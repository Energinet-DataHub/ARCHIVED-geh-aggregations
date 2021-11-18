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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.Helpers;
using Moq;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests
{
    [UnitTest]
    public class BlobStoreTests
    {
        private readonly BlobStore _sut;

        public BlobStoreTests()
        {
            _sut = new BlobStore();
        }

        [Fact]
        public async Task DownloadFromBlobContainer_ReturnsResultFromBlobContainer()
        {
            // var mock = BlobsModelFactory.BlobItem("mock");
            var connectionString = "UseDevelopmentStorage=true";
            var containerName = "result-data";
            var blobName = "result_mock_flex_consumption_per_grid_area.json";

            var actual = await _sut.DownloadFromBlobContainerAsync(connectionString, containerName, blobName).ConfigureAwait(false);
            var testDataGenerator = new TestDataGenerator();
            var expected = testDataGenerator.EmbeddedResourceAssetReader("result_mock_flex_consumption_per_grid_area.json");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task UploadToBlobContainerAsync_SavesResultToBlobContainer()
        {
            // mock a blobcontainer and see if upload method works
            // var mock = BlobsModelFactory.BlobcontainerC("mock-result-data", null);
            var connectionString = "UseDevelopmentStorage=true";
            var containerName = "cim-xml-data";
            var blobName = "shitJustWorks.xml";

            var stream = new MemoryStream();
            var doc = new XDocument(new XElement("test", "test"));
            await doc.SaveAsync(stream, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);
            stream.Position = 0;
            var message = await _sut.UploadStreamToBlobContainerAsync(connectionString, containerName, blobName, stream).ConfigureAwait(false);
            var blockBlobClient = new BlockBlobClient(connectionString, containerName, blobName);
            var exist = (await blockBlobClient.ExistsAsync().ConfigureAwait(false)).Value;
            Assert.True(exist);
        }

        [Fact]
        public async Task DeleteFromBlobContainerAsync_DeleteResultFromBlobContainer()
        {
            // mock a blobcontainer and see if upload method works
            // var mock = BlobsModelFactory.BlobcontainerC("mock-result-data", null);
            var connectionString = "UseDevelopmentStorage=true";
            var containerName = "cim-xml-data";
            var blobName = "shitJustWorks.xml";

            var message = await _sut.DeleteFromBlobContainerAsync(connectionString, containerName, blobName).ConfigureAwait(false);
            var blockBlobClient = new BlockBlobClient(connectionString, containerName, blobName);
            var exist = (await blockBlobClient.ExistsAsync().ConfigureAwait(false)).Value;
            Assert.False(exist);
        }
    }
}
