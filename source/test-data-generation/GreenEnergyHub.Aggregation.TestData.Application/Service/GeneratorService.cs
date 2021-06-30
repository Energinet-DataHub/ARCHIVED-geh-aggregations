using System;
using System.Collections.Generic;
using System.IO;
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
