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

using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain.MeteringPoints;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Aggregations.Infrastructure.Persistence
{
    /// <summary>
    /// The interface for the MasterDataContext
    /// </summary>
    public interface IMasterDataDbContext
    {
        /// <summary>
        /// DB access to the metering points
        /// </summary>
        DbSet<MeteringPoint> MeteringPoints { get; }

        /// <summary>
        /// .
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
