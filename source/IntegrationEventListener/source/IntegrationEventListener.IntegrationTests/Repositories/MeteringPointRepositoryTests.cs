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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData.MeteringPoints;
using Energinet.DataHub.Aggregations.Infrastructure.Persistence.Repositories;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.IntegrationTest.Core.Fixtures;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Repositories
{
    public class MeteringPointRepositoryTests : IClassFixture<MasterDataDatabaseFixture>
    {
        private readonly MasterDataDatabaseManager _databaseManager;

        public MeteringPointRepositoryTests(MasterDataDatabaseFixture fixture)
        {
            if (fixture == null)
            {
                throw new ArgumentNullException(nameof(fixture));
            }

            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task WhenMeteringPointIsAddedOrUpdatedThenMeteringPointCanBeFetchedFromDatabaseAsync(
            string id,
            Instant effectiveDate)
        {
            // Arrange
            var mp = new MeteringPoint
            {
                RowId = Guid.NewGuid(),
                MeteringPointId = id,
                MeteringPointType = MeteringPointType.Consumption,
                ConnectionState = ConnectionState.New,
                GridArea = "1",
                MeteringMethod = MeteringMethod.Physical,
                Product = Product.EnergyActive,
                Resolution = Resolution.Hourly,
                FromDate = effectiveDate,
                ToDate = effectiveDate.PlusNanoseconds(1000),
                Unit = Unit.Gwh,
                SettlementMethod = SettlementMethod.Flex,
                InGridArea = "InGridArea",
                OutGridArea = "OutGridArea",
                ParentMeteringPointId = "ParentMeteringPointId",
            };

            await using var writeContext = _databaseManager.CreateDbContext();
            var writeSut = new MeteringPointRepository(writeContext);

            // Act
            await writeSut.AddOrUpdateAsync(new List<MeteringPoint> { mp }).ConfigureAwait(false);

            // Assert
            await using var readContext = _databaseManager.CreateDbContext();
            var readSut = new MeteringPointRepository(readContext);
            var actual = await readSut.GetByIdAndDateAsync(id, effectiveDate).ConfigureAwait(false);

            Assert.NotNull(actual);
            Assert.True(actual.FirstOrDefault(x => x.MeteringPointId == id && x.FromDate == effectiveDate) != null);
        }
    }
}
