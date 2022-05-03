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
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.DatabaseMigration;
using Energinet.DataHub.Aggregations.Infrastructure.Persistence;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Database;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.IntegrationTest.Core.Fixtures
{
    public class MasterDataDatabaseManager : SqlServerDatabaseManager<MasterDataDbContext>
    {
        public MasterDataDatabaseManager()
            : base("AggregationMasterData")
        {
        }

        /// <inheritdoc/>
        public override MasterDataDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<MasterDataDbContext>();
            var dbContextOptions = optionsBuilder
                .UseSqlServer(ConnectionString, options => options.UseNodaTime());

            return new MasterDataDbContext(optionsBuilder.Options);
        }

        /// <summary>
        /// Creates the database schema using DbUp instead of a database context.
        /// </summary>
        protected override Task<bool> CreateDatabaseSchemaAsync(MasterDataDbContext context)
        {
            return Task.FromResult(CreateDatabaseSchema(context));
        }

        /// <summary>
        /// Creates the database schema using DbUp instead of a database context.
        /// </summary>
        protected override bool CreateDatabaseSchema(MasterDataDbContext context)
        {
            var result = Upgrader.DatabaseUpgrade(ConnectionString);
            if (result.Successful is false)
                throw new Exception("Database migration failed", result.Error);

            return true;
        }
    }
}
