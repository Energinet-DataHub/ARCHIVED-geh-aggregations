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
using Energinet.DataHub.Aggregations.Domain.MasterData.MeteringPoints;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Infrastructure.Persistence.Repositories
{
    public class MeteringPointRepository : IMeteringPointRepository
    {
        private readonly IMasterDataDbContext _dbContext;

        public MeteringPointRepository(IMasterDataDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<MeteringPoint>> GetByIdAndDateAsync(string id, Instant effectiveDate)
        {
            return await _dbContext.MeteringPoints
                .Where(x => x.MeteringPointId == id && x.ToDate > effectiveDate)
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task AddOrUpdateAsync(List<MeteringPoint> masterDataObjects)
        {
            if (masterDataObjects == null) throw new ArgumentNullException(nameof(masterDataObjects));

            foreach (var meteringPoint in masterDataObjects)
            {
                var existing = _dbContext.MeteringPoints
                    .FirstOrDefault(x => x.RowId == meteringPoint.RowId);

                if (existing == null)
                {
                    await _dbContext.MeteringPoints.AddAsync(meteringPoint).ConfigureAwait(false);
                }
            }

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
