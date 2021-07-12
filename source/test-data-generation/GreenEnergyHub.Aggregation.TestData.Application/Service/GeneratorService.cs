using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.TestData.Application.Service
{
    public class GeneratorService : IGeneratorService
    {
        private readonly IEnumerable<ITestDataParser> _testDataParsers;
        private readonly ILogger<GeneratorService> _logger;

        public GeneratorService(IEnumerable<ITestDataParser> testDataParsers, ILogger<GeneratorService> logger)
        {
            _testDataParsers = testDataParsers;
            _logger = logger;
        }

        public async Task HandleChangedFileAsync(Stream myblob, string name)
        {
            try
            {
                var parser = _testDataParsers.SingleOrDefault(p => p.FileNameICanHandle.ToUpperInvariant() == name.ToUpperInvariant().Trim());
                if (parser == null)
                {
                    _logger.LogWarning($"Could not find a parser for {name}");
                    return;
                }

                await parser.ParseAsync(myblob).ConfigureAwait(false);
                _logger.LogInformation("Seems that we send data successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong when handling changed file");
            }
        }
    }
}
