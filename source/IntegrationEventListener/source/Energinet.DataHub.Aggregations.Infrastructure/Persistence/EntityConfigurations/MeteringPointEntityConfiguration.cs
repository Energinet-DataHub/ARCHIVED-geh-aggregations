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
            builder.Property(x => x.RowId).ValueGeneratedNever();
            builder.Property(x => x.MeteringPointId);
            builder.Property(x => x.MeteringPointType);
            builder.Property(x => x.GridArea);
            builder.Property(x => x.ConnectionState);
            builder.Property(x => x.Resolution);
            builder.Property(x => x.MeteringMethod);
            builder.Property(x => x.Unit);
            builder.Property(x => x.FromDate);
            builder.Property(x => x.ToDate);
        }
    }
}
