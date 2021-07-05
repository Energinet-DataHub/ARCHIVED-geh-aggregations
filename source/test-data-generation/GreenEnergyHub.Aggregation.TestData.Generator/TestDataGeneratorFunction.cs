﻿// Copyright 2020 Energinet DataHub A/S
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

using System.IO;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Application.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.TestData.GeneratorFunction
{
    public class TestDataGeneratorFunction
    {
        private readonly IGeneratorService _generatorService;
        private readonly ILogger<TestDataGeneratorFunction> _logger;

        public TestDataGeneratorFunction(IGeneratorService generatorService, ILogger<TestDataGeneratorFunction> logger)
        {
            _generatorService = generatorService;
            _logger = logger;
        }

        [FunctionName("BlobTrigger")]
        public async Task RunAsync([BlobTrigger("test-data-source/{name}", Connection = "TEST_DATA_SOURCE_CONNECTION_STRING")]Stream myblob, string name, ILogger log)
        {
            _logger.LogInformation("Triggered blobtrigger");

            await _generatorService.HandleChangedFileAsync(myblob, name).ConfigureAwait(false);
        }
    }
}
