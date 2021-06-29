using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.TestData.Application.Service
{
    public class GeneratorService : IGeneratorService
    {
        private readonly IMasterDataStorage _masterDataStorage;
        private readonly IEnumerable<ITestDataParser> _testDataParsers;
        private readonly ILogger<GeneratorService> _logger;

        public GeneratorService(IMasterDataStorage masterDataStorage, IEnumerable<ITestDataParser> testDataParsers, ILogger<GeneratorService> logger)
        {
            _masterDataStorage = masterDataStorage;
            _testDataParsers = testDataParsers;
            _logger = logger;
        }

        public async Task HandleChangedFileAsync(Stream myblob, string name)
        {
            var parser = _testDataParsers.SingleOrDefault(p => p.FileNameICanHandle == name);
            if (parser == null)
            {
                _logger.LogInformation($"Could not find a parser for {name}");
                return;
            }

            await parser.ParseAsync(myblob).ConfigureAwait(false);
        }
    }
}
