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
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.Aggregations.Application.Extensions;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Aggregations.Infrastructure.Persistence;
using Energinet.DataHub.Aggregations.Infrastructure.Repository.InsertUpdates;
using Energinet.DataHub.Aggregations.Infrastructure.Repository.SqlInstructions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using MeteringPoint = Energinet.DataHub.Aggregations.Infrastructure.Persistence.MeteringPoint;

namespace Energinet.DataHub.Aggregations.Infrastructure.Repository
{
    /// <summary>
    /// This represents a reposiory for manipulating IMasterDataObjects
    /// If you want to add a new master data object. Implement a new ISqlInstructions and add it to the _insertUpdate dictonary.
    /// The repository will take care of the rest
    /// </summary>
    public class MeteringPointRepository : IMasterDataRepository<MeteringPoint>
    {
        private readonly MasterDataContext _dbContext;

        public MeteringPointRepository(MasterDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<MeteringPoint>> GetByIdAndDateAsync(string id, Instant effectiveDate)
        {
            var mp = _dbContext.MeteringPoint.Where(x =>
                x.MeteringPointId == id && x.ToDate > effectiveDate.ToDateTimeUtc());

            return await mp.ToListAsync().ConfigureAwait(false);

            //await using var conn = await GetConnectionAsync().ConfigureAwait(false);

            //var sqlInstructions = _sqlInstructions[typeof(MeteringPoint)];

            //var results = await conn
            //    .QueryAsync<MeteringPoint>(
            //        sqlInstructions.Get,
            //        new { id, effectiveDate = effectiveDate.ToIso8601GeneralString() }).ConfigureAwait(false);
            //return results.ToList();
        }

        public async Task AddOrUpdateAsync(List<MeteringPoint> masterDataObjects)
        {
            if (masterDataObjects == null) throw new ArgumentNullException(nameof(masterDataObjects));
            foreach (var masterDataObject in masterDataObjects)
            {
                if (_dbContext.MeteringPoint.SingleOrDefault(x => x.RowId == masterDataObject.RowId) != null)
                {
                    await InsertMasterDataAsync(masterDataObject).ConfigureAwait(false);
                }
                else
                {
                    await UpdateMasterDataObjectAsync(masterDataObject).ConfigureAwait(false);
                }
            }

            //await using var conn = await GetConnectionAsync().ConfigureAwait(false);

            //foreach (var masterData in masterDataObjects)
            //{
            //    if (await MasterDataExistsAsync<MeteringPoint>(masterData.RowId, conn).ConfigureAwait(false))
            //    {
            //        await UpdateMasterDataObjectAsync(masterData, conn).ConfigureAwait(false);
            //    }
            //    else
            //    {
            //        await InsertMasterDataAsync(masterData, conn).ConfigureAwait(false);
            //    }
            //}
        }

        //private static async Task<bool> MasterDataExistsAsync<T>(Guid rowId, SqlConnection conn)
        //    where T : IMasterDataObject
        //{
        //    var count = await conn.ExecuteScalarAsync<int>(
        //        $"SELECT COUNT(1) FROM dbo.{typeof(T).Name} md WHERE md.RowId = @rowId",
        //        new { rowId }).ConfigureAwait(false);

        //    return count > 0;
        //}
        private async Task InsertMasterDataAsync(MeteringPoint masterDataObject)
        {
            _dbContext.MeteringPoint.Add(masterDataObject);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            //await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            //var insertUpdateInstructions = _sqlInstructions[typeof(T)];

            //await conn.ExecuteAsync(insertUpdateInstructions.Insert, transaction: transaction, param: insertUpdateInstructions.InsertParameters(masterDataObject)).ConfigureAwait(false);
            //await transaction.CommitAsync().ConfigureAwait(false);
        }

        private async Task UpdateMasterDataObjectAsync(MeteringPoint masterDataObject)
        {
            _dbContext.MeteringPoint.Update(masterDataObject);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            //await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);

            //var insertUpdateInstructions = _sqlInstructions[typeof(T)];

            //await conn.ExecuteAsync(insertUpdateInstructions.Update, transaction: transaction, param: insertUpdateInstructions.UpdateParameters(masterDataObject)).ConfigureAwait(false);
            //await transaction.CommitAsync().ConfigureAwait(false);
        }

        //private async Task<SqlConnection> GetConnectionAsync()
        //{
        //    var conn = new SqlConnection(_connectionString);
        //    await conn.OpenAsync().ConfigureAwait(false);
        //    return conn;
        //}
    }
}
