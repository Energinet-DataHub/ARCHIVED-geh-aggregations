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

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using GreenEnergyHub.Aggregation.TestData.Infrastructure;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;

namespace GreenEnergyHub.Aggregation.TestData.Application.Parsers
{
    public abstract class TestDataParserBase<T>
        where T : IStoragebleObject
    {
        private readonly GeneratorSettings _generatorSettings;
        private readonly IMasterDataStorage _masterDataStorage;

        protected TestDataParserBase(IMasterDataStorage masterDataStorage, GeneratorSettings generatorSettings)
        {
            _generatorSettings = generatorSettings;
            _masterDataStorage = masterDataStorage;
        }

        public async Task ParseAsync(Stream stream)
        {
            using var tr = new StreamReader(stream);
            using var csv = new CsvReader(tr, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
            });

            var containerName = typeof(T) switch
            {
                var cls when cls == typeof(Charge) => _generatorSettings.ChargesContainerName,
                var cls when cls == typeof(ChargeLink) => _generatorSettings.ChargeLinkContainerName,
                var cls when cls == typeof(ChargePrices) => _generatorSettings.ChargePriceContainerName,
                var cls when cls == typeof(MeteringPoint) => _generatorSettings.MeteringPointContainerName,
                var cls when cls == typeof(MarketRole) => _generatorSettings.MarketRolesContainerName,
                var cls when cls == typeof(SpecialMeteringPoint) => _generatorSettings.SpecialMeteringPointContainerName,
                _ => throw new ArgumentException($"Could not find container for {typeof(T).Name}")
            };
            var records = csv.GetRecordsAsync<T>();

            await _masterDataStorage.PurgeContainerAsync(containerName).ConfigureAwait(false);
            await _masterDataStorage.WriteAsync(records, containerName).ConfigureAwait(false);
        }
    }
}
