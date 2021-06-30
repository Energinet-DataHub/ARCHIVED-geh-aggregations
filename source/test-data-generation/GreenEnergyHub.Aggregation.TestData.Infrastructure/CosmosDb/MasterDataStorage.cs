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

using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;
using Microsoft.Azure.Cosmos;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb
{
    public class MasterDataStorage : IMasterDataStorage
    {
        private const string DatabaseId = "master-data";
        private readonly GeneratorSettings _generatorSettings;
        private readonly CosmosClient _client;

        public MasterDataStorage(GeneratorSettings generatorSettings)
        {
            _generatorSettings = generatorSettings;
            _client = new CosmosClient(generatorSettings.MasterDataStorageConnectionString);
        }

        public async Task WriteMeteringPointAsync(MeteringPoint mp)
        {
            var container = _client.GetContainer(DatabaseId, _generatorSettings.MeteringPointContainerName);
            await container.CreateItemAsync(mp).ConfigureAwait(false);
        }
    }
}
