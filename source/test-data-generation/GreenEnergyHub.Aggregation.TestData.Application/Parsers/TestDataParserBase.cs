using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Application.Service;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb;

namespace GreenEnergyHub.Aggregation.TestData.Application.Parsers
{
    public abstract class TestDataParserBase : ITestDataParser
    {
        protected TestDataParserBase(IMasterDataStorage masterDataStorage)
        {
            MasterDataStorage = masterDataStorage;
        }

        public abstract string FileNameICanHandle { get; }

        protected IMasterDataStorage MasterDataStorage { get; }

        public abstract Task ParseAsync(Stream stream);
    }
}
