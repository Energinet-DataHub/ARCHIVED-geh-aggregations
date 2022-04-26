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
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Aggregations.Domain.MasterData.MeteringPoint;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Aggregations.Infrastructure.Persistence.EntityConfigurations
{
    public class MeteringPointEntityConfiguration : IEntityTypeConfiguration<MeteringPoint>
    {
        public void Configure(EntityTypeBuilder<MeteringPoint> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable(nameof(MeteringPoint));

            builder.HasKey(x => x.RowId).IsClustered(false);
            builder.Property(x => x.RowId)
                .IsRequired()
                .HasColumnType("nvarchar(50)")
                .ValueGeneratedNever();

            builder.Property(x => x.MeteringPointId)
                .IsRequired()
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.MeteringPointType)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.GridArea)
                .IsRequired()
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.ConnectionState)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.Resolution)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.MeteringMethod)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.Unit)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.FromDate)
                .HasColumnType("datetime2(7)")
                .IsRequired();

            builder.Property(x => x.ToDate)
                .HasColumnType("datetime2(7)")
                .IsRequired();

            builder.Property(x => x.SettlementMethod)
                .HasColumnType("int");

            builder.Property(x => x.InGridArea)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.OutGridArea)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.ParentMeteringPointId)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.Product)
                .HasColumnType("int");
        }
    }
}
