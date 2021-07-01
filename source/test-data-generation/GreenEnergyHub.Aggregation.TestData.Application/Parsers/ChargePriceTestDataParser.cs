using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Application.Service;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb;

namespace GreenEnergyHub.Aggregation.TestData.Application.Parsers
{
    public class ChargePriceTestDataParser : TestDataParserBase<ChargePrices>, ITestDataParser
    {
        public ChargePriceTestDataParser(IMasterDataStorage masterDataStorage)
            : base(masterDataStorage)
        {
        }

        public override string FileNameICanHandle => "ChargePrices.csv";
    }
}
