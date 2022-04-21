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
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DbUp;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Aggregations.Infrastructure.Repository;
using NodaTime;
using ThrowawayDb;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.IntegrationTest
{
    [UnitTest]
    public class EventToMasterDataTransformerTests : IDisposable
    {
        private readonly ThrowawayDatabase _database;
        private readonly IMasterDataRepository<MeteringPoint> _masterDataRepository;

        public EventToMasterDataTransformerTests()
        {
            _database = ThrowawayDatabase.FromLocalInstance(".\\SQLEXPRESS");

            Console.WriteLine($"Created database {_database.Name}");
            var upgrader =
                DeployChanges.To
                    .SqlDatabase(_database.ConnectionString)
                    .WithScriptsAndCodeEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();
            //TODO we cant find the upgrade scripts in the assembly
            var s = upgrader.GetDiscoveredScripts();
            upgrader.PerformUpgrade();
            _masterDataRepository = new MeteringPointRepository(_database.ConnectionString);
        }

        [Fact]
        public async Task TestCreationOfMeteringPoint()
        {
            const string MeteringPointId = "test";
            var mp = new MeteringPoint { Id = MeteringPointId, FromDate = Instant.MinValue, ToDate = Instant.MaxValue };
            await _masterDataRepository.AddOrUpdateAsync(new List<MeteringPoint>() { mp });
            var list = await _masterDataRepository.GetByIdAndDateAsync(MeteringPointId, Instant.MinValue);
            Assert.Contains(list, point => point.Id == MeteringPointId);
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
