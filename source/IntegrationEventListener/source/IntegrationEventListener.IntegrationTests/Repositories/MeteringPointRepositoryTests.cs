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
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData.MeteringPoint;
using Energinet.DataHub.Aggregations.Infrastructure.Persistence.Repositories;
using Energinet.DataHub.IntegrationTest.Core.Fixtures;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Repositories
{
    public class MeteringPointRepositoryTests : IClassFixture<MasterDataDatabaseFixture>
    {
        private readonly MasterDataDatabaseManager _databaseManager;

        public MeteringPointRepositoryTests(MasterDataDatabaseFixture fixture)
        {
            if (fixture == null) throw new ArgumentNullException(nameof(fixture));

            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task Test()
        {
            var mp = new MeteringPoint()
            {
                RowId = Guid.NewGuid(),
                MeteringPointId = "1",
                MeteringPointType = MeteringPointType.Consumption,
                ConnectionState = ConnectionState.New,
                GridArea = "1",
                MeteringMethod = MeteringMethod.Physical,
                Product = Product.EnergyActive,
                Resolution = Resolution.Hourly,
                FromDate = Instant.FromUtc(2020, 1, 1, 0, 0),
                ToDate = Instant.FromUtc(2022, 1, 1, 0, 0),
            };

            await using var context = _databaseManager.CreateDbContext();

            var sut = new MeteringPointRepository(context);

            await sut.AddOrUpdateAsync(new List<MeteringPoint>() { mp }).ConfigureAwait(false);
        }
    }
}
