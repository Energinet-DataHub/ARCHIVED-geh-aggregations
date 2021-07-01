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
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb
{
    public class MasterDataStorage : IMasterDataStorage
    {
        private const string DatabaseId = "master-data";
        private readonly GeneratorSettings _generatorSettings;
        private readonly ILogger<MasterDataStorage> _logger;
        private readonly CosmosClient _client;

        public MasterDataStorage(GeneratorSettings generatorSettings, ILogger<MasterDataStorage> logger)
        {
            _generatorSettings = generatorSettings;
            _logger = logger;
            _client = new CosmosClient(generatorSettings.MasterDataStorageConnectionString);
        }

        public async Task WriteMeteringPointAsync(MeteringPoint meteringPoint)
        {
            var container = _client.GetContainer(DatabaseId, _generatorSettings.MeteringPointContainerName);
            await container.CreateItemAsync(meteringPoint).ConfigureAwait(false);
        }

        public async Task WriteMeteringPointsAsync(IAsyncEnumerable<MeteringPoint> meteringPoints)
        {
            try
            {
                var container = _client.GetContainer(DatabaseId, _generatorSettings.MeteringPointContainerName);
                //TODO can this be optimized ?
                await foreach (var meteringPoint in meteringPoints)
                {
                    var mp = meteringPoint;
                    await container.CreateItemAsync(meteringPoint).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not put item in cosmos");
            }
        }
    }
}
