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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using GreenEnergyHub.Aggregation.TestData.Infrastructure;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Extensions;
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
                Delimiter = ",",
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                ShouldSkipRecord = (r) => r.Record.All(string.IsNullOrEmpty),
            });

            string partitionKey;
            string containerName;
            switch (typeof(T))
            {
                case var cls when cls == typeof(Charge):
                    containerName = _generatorSettings.ChargesContainerName;
                    partitionKey = "charge_key";
                    break;
                case var cls when cls == typeof(ChargeLink):
                    containerName = _generatorSettings.ChargeLinkContainerName;
                    partitionKey = "charge_key";
                    break;
                case var cls when cls == typeof(ChargePrices):
                    containerName = _generatorSettings.ChargePriceContainerName;
                    partitionKey = "charge_key";
                    break;
                case var cls when cls == typeof(MeteringPoint):
                    containerName = _generatorSettings.MeteringPointContainerName;
                    partitionKey = "metering_point_id";
                    break;
                case var cls when cls == typeof(MarketRole):
                    containerName = _generatorSettings.MarketRolesContainerName;
                    partitionKey = "energy_supplier_id";
                    break;
                case var cls when cls == typeof(SpecialMeteringPoint):
                    containerName = _generatorSettings.SpecialMeteringPointContainerName;
                    partitionKey = "grid_area";
                    break;
                case var cls when cls == typeof(BalanceResponsiblePartyRelation):
                    containerName = _generatorSettings.BalanceResponsiblePartyRelationContainerName;
                    partitionKey = "grid_area";
                    break;
                default:
                    throw new ArgumentException($"Could not find container for {typeof(T).Name}");
            }

            var records = csv.GetRecordsAsync<T>();

            var recordsList = await records.ToListAsync().ConfigureAwait(false);

            await _masterDataStorage.PurgeContainerAsync(containerName, partitionKey).ConfigureAwait(false);

            for (var i = 0; i < recordsList.Count; i = i + 100)
            {
                var items = recordsList.Skip(i).Take(100);
                await _masterDataStorage.WriteAsync(items, containerName).ConfigureAwait(false);
            }
        }
    }
}
