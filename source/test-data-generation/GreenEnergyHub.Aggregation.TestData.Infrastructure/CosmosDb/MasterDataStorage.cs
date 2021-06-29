using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb
{
    public class MasterDataStorage : IMasterDataStorage
    {
        private string _baseConnectionString;

        public MasterDataStorage(GeneratorSettings generatorSettings)
        {
            _baseConnectionString = generatorSettings.MasterDataStorageConnectionString;
        }
    }
}
