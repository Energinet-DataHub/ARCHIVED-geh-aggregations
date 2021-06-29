using System;
using System.Collections.Generic;
using System.Text;
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
