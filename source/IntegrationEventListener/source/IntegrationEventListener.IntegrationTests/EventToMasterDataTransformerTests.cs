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
using System.Linq;
using System.Threading.Tasks;
using Dapper.NodaTime;
using Energinet.DataHub.Aggregations.Application;
using Energinet.DataHub.Aggregations.Application.Extensions;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.DTOs.MeteringPoints;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.Mutators;
using Energinet.DataHub.Aggregations.DatabaseMigration;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Aggregations.Infrastructure.Repository;
using NodaTime;
using ThrowawayDb;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests
{
    [IntegrationTest]
    public class EventToMasterDataTransformerTests : IDisposable
    {
        private readonly ThrowawayDatabase _database;
        private readonly EventToMasterDataTransformer<MeteringPointCreatedMutator, MeteringPoint> _eventToMasterDataTransformer;
        private readonly MeteringPointRepository _meteringPointRepository;
        private bool _disposed;

        public EventToMasterDataTransformerTests()
        {
            _database = ThrowawayDatabase.FromLocalInstance("(localdb)\\mssqllocaldb");
            Console.WriteLine($"Created database {_database.Name}");

            var result = Upgrader.DatabaseUpgrade(_database.ConnectionString);
            Assert.True(result.Successful);
            _meteringPointRepository = new MeteringPointRepository(_database.ConnectionString);
            DapperNodaTimeSetup.Register();
            _eventToMasterDataTransformer =
                new EventToMasterDataTransformer<MeteringPointCreatedMutator, MeteringPoint>(_meteringPointRepository);
        }

        [Fact]
        public async Task TestCreationOfMeteringPointViaEventToMasterDataTransformerAndRetrieveFromDb()
        {
            const string mpid = "mpid";
            var effectiveDate = SystemClock.Instance.GetCurrentInstant();
            var evt = new MeteringPointCreatedEvent(
                mpid,
                MeteringPointType.Consumption,
                "gridarea",
                SettlementMethod.Flex,
                MeteringMethod.Calculated,
                Resolution.Hourly,
                Product.EnergyActive,
                ConnectionState.New,
                Unit.Kwh,
                effectiveDate);

            await _eventToMasterDataTransformer.HandleTransformAsync(new MeteringPointCreatedMutator(evt)).ConfigureAwait(false);

            var mp = (await _meteringPointRepository.GetByIdAndDateAsync(mpid, effectiveDate).ConfigureAwait(false)).SingleOrDefault();

            Assert.NotNull(mp);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.Equal(evt.MeteringPointId, mp.MeteringPointId);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Assert.Equal(evt.MeteringPointType, mp.MeteringPointType);
            Assert.Equal(evt.GridArea, mp.GridArea);
            Assert.Equal(evt.SettlementMethod, mp.SettlementMethod);
            Assert.Equal(evt.MeteringMethod, mp.MeteringMethod);
            Assert.Equal(evt.Resolution, mp.Resolution);
            Assert.Equal(evt.Product, mp.Product);
            Assert.Equal(evt.ConnectionState, mp.ConnectionState);
            Assert.Equal(evt.Unit, mp.Unit);
            Assert.Equal(evt.EffectiveDate.ToIso8601GeneralString(), mp.FromDate.ToIso8601GeneralString());
            Assert.Equal(Instant.MaxValue.ToIso8601GeneralString(), mp.ToDate.ToIso8601GeneralString());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _database.Dispose();
            _disposed = true;
        }
    }
}
