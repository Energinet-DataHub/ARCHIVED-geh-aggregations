using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Application.Service;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;

namespace GreenEnergyHub.Aggregation.TestData.Application.Parsers
{
    public class MeteringPointTestDataParser : TestDataParserBase
    {
        public MeteringPointTestDataParser(IMasterDataStorage masterDataStorage)
            : base(masterDataStorage)
        {
        }

        public override string FileNameICanHandle => "MeteringPoints.csv";

        public override async Task ParseAsync(Stream stream)
        {
            var mp = new MeteringPoint("123");
            await MasterDataStorage.WriteMeteringPointAsync(mp).ConfigureAwait(false);
        }
    }
}
