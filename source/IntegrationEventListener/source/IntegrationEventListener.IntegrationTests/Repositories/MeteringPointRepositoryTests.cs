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
using Energinet.DataHub.Aggregations.Domain.MeteringPoints;
using Energinet.DataHub.Aggregations.Infrastructure.Persistence.Repositories;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.IntegrationTest.Core.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

        [Theory]
        [InlineAutoMoqData]
        public async Task AddOrUpdateAsync_StoresMeteringPoint(
            string meteringPointId,
            Instant effectiveDate)
        {
            // Arrange
            meteringPointId = meteringPointId[..50];
            var meteringPoint = CreateMeteringPoint(meteringPointId, effectiveDate);

            await using var writeContext = _databaseManager.CreateDbContext();
            var sut = new MeteringPointRepository(writeContext);

            // Act
            await sut.AddOrUpdateAsync(new List<MeteringPoint> { meteringPoint }).ConfigureAwait(false);

            // Assert
            await using var readContext = _databaseManager.CreateDbContext();
            var actual = await readContext.MeteringPoints.SingleAsync(x => x.RowId == meteringPoint.RowId).ConfigureAwait(false);

            actual.Should().NotBeNull();
            actual.MeteringPointId.Should().Be(meteringPointId);
            actual.FromDate.Should().Be(effectiveDate);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetByIdAndDateAsync_ReturnsMeteringPoint(
            string meteringPointId,
            Instant effectiveDate)
        {
            // Arrange
            meteringPointId = meteringPointId[..50];
            var meteringPoint = CreateMeteringPoint(meteringPointId, effectiveDate);

            await using var writeContext = _databaseManager.CreateDbContext();
            var sut = new MeteringPointRepository(writeContext);
            await sut.AddOrUpdateAsync(new List<MeteringPoint> { meteringPoint }).ConfigureAwait(false);

            // Act
            var actual = await sut.GetByIdAndDateAsync(meteringPointId, effectiveDate).ConfigureAwait(false);

            // Assert
            actual.First().Should().NotBeNull();
            actual.First().MeteringPointId.Should().Be(meteringPointId);
            actual.First().FromDate.Should().Be(effectiveDate);
        }

        private static MeteringPoint CreateMeteringPoint(string meteringPointId, Instant effectiveDate)
        {
            return new MeteringPoint
            {
                MeteringPointId = meteringPointId,
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
        }
    }
}
