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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain.MasterData.MeteringPoints;
using Energinet.DataHub.Aggregations.Infrastructure.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Aggregations.Infrastructure.Persistence
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Private setters are needed by EF Core")]
    public class MasterDataDbContext : DbContext
    {
        #nullable disable
        public MasterDataDbContext(DbContextOptions<MasterDataDbContext> options)
            : base(options)
        {
        }

        public DbSet<MeteringPoint> MeteringPoints { get; private set; }

        public Task<int> SaveChangesAsync()
            => base.SaveChangesAsync();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.ApplyConfiguration(new MeteringPointEntityConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
