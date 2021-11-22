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

using Azure.Storage.Blobs.Specialized;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Helpers;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Helpers
{
    public class BlockBlobClientGenerator : IBlockBlobClientGenerator
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlockBlobClientGenerator(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            _containerName = containerName;
        }

        public BlockBlobClient GetBlockBlobClient(string blobName)
        {
            return new BlockBlobClient(_connectionString, _containerName, blobName);
        }
    }
}
