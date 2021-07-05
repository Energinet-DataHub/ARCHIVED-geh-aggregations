using System;
using System.Collections.Generic;
using System.Text;
using GreenEnergyHub.Aggregation.TestData.Application.Service;
using GreenEnergyHub.Aggregation.TestData.Infrastructure;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;

namespace GreenEnergyHub.Aggregation.TestData.Application.Parsers
{
    public class SpecialMeteringPointsTestParser : TestDataParserBase<SpecialMeteringPoint>, ITestDataParser
    {
        public SpecialMeteringPointsTestParser(IMasterDataStorage masterDataStorage, GeneratorSettings generatorSettings)
            : base(masterDataStorage, generatorSettings)
        {
        }

        public override string FileNameICanHandle => "GL&SKMP(Master).csv";
    }
}
