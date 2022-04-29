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

using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Aggregations.Domain.MeteringPoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Aggregations.Infrastructure.Persistence.EntityConfigurations
{
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "builder is not null")]
    public class MeteringPointEntityConfiguration : IEntityTypeConfiguration<MeteringPoint>
    {
        public void Configure(EntityTypeBuilder<MeteringPoint> builder)
        {
            builder.ToTable(nameof(MeteringPoint));

            builder.HasKey(x => x.RowId).IsClustered(false);
            builder.Property(x => x.RowId)
                .IsRequired()
                .HasColumnName("RowId")
                .HasColumnType("uniqueidentifier")
                .HasDefaultValue();

            builder.Property(x => x.MeteringPointId)
                .IsRequired()
                .HasColumnName("MeteringPointId")
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.MeteringPointType)
                .IsRequired()
                .HasColumnName("MeteringPointType")
                .HasColumnType("int");

            builder.Property(x => x.GridArea)
                .IsRequired()
                .HasColumnName("GridArea")
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.ConnectionState)
                .IsRequired()
                .HasColumnName("ConnectionState")
                .HasColumnType("int");

            builder.Property(x => x.Resolution)
                .IsRequired()
                .HasColumnName("Resolution")
                .HasColumnType("int");

            builder.Property(x => x.MeteringMethod)
                .HasColumnName("MeteringMethod")
                .HasColumnType("int");

            builder.Property(x => x.Unit)
                .IsRequired()
                .HasColumnName("Unit")
                .HasColumnType("int");

            builder.Property(x => x.FromDate)
                .IsRequired()
                .HasColumnName("FromDate")
                .HasColumnType("datetime2(7)");

            builder.Property(x => x.ToDate)
                .IsRequired()
                .HasColumnName("ToDate")
                .HasColumnType("datetime2(7)");

            builder.Property(x => x.SettlementMethod)
                .HasColumnName("SettlementMethod")
                .HasColumnType("int");

            builder.Property(x => x.InGridArea)
                .HasColumnName("InGridArea")
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.OutGridArea)
                .HasColumnName("OutGridArea")
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.ParentMeteringPointId)
                .HasColumnName("ParentMeteringPointId")
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            builder.Property(x => x.Product)
                .HasColumnName("Product")
                .HasColumnType("int");
        }
    }
}
