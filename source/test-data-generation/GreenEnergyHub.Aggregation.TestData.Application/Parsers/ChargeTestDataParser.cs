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
    public class ChargeTestDataParser : TestDataParserBase, ITestDataParser
    {
        public ChargeTestDataParser(IMasterDataStorage masterDataStorage)
            : base(masterDataStorage)
        {
        }

        public override string FileNameICanHandle => "Charges";

        public override async Task ParseAsync(Stream stream)
        {
            var charge = new Charge();
            await MasterDataStorage.WriteChargeAsync(charge).ConfigureAwait(false);
        }
    }
}
