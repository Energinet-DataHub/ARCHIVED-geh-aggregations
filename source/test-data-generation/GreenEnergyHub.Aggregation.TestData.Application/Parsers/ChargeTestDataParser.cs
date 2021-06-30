using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
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

        public override string FileNameICanHandle => "charges.csv";

        public override async Task ParseAsync(Stream stream)
        {
            using var tr = new StreamReader(stream);
            using var csv = new CsvReader(tr, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
            });
            var records = csv.GetRecordsAsync<Charge>();
            await MasterDataStorage.WriteChargesAsync(records).ConfigureAwait(false);
        }
    }
}
