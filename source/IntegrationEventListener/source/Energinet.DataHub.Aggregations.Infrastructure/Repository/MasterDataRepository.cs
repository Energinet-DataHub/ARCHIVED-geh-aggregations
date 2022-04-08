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
using Energinet.DataHub.Aggregations.Infrastructure.Repository.InsertUpdates;
using Energinet.DataHub.Aggregations.Infrastructure.Repository.SqlInstructions;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Infrastructure.Repository
{
    /// <summary>
    /// This represents a reposiory for manipulating IMasterDataObjects
    /// If you want to add a new master data object. Implement a new ISqlInstructions and add it to the _insertUpdate dictonary.
    /// The repository will take care of the rest
    /// </summary>
    public class MasterDataRepository : IMasterDataRepository
    {
        private readonly string _connectionString;
        private readonly IDictionary<Type, ISqlInstructions<IMasterDataObject>> _sqlInstructions;

        public MasterDataRepository(string connectionString)
        {
            _connectionString = connectionString;
            _sqlInstructions = new Dictionary<Type, ISqlInstructions<IMasterDataObject>>();

            // Insert new master data manipulations to the list here
            _sqlInstructions.Add(typeof(MeteringPoint), new MeteringPointSqlInstructions<MeteringPoint>());
        }

        public async Task<List<T>> GetByIdAndDateAsync<T>(string id, Instant effectiveDate)
            where T : IMasterDataObject
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);

            var sqlInstructions = _sqlInstructions[typeof(T)];

            var results = await conn
                .QueryAsync<T>(
                    sqlInstructions.GetSql,
                    new { id, effectiveDate = effectiveDate.ToIso8601GeneralString() }).ConfigureAwait(false);

            return results.ToList();
        }

        public async Task AddOrUpdateMeteringPointsAsync<T>(List<T> masterDataObjects)
            where T : IMasterDataObject
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);

            foreach (var masterData in masterDataObjects)
            {
                if (await MasterDataExistsAsync<T>(masterData.RowId, conn).ConfigureAwait(false))
                {
                    await UpdateMasterDataObjectAsync(masterData, conn).ConfigureAwait(false);
                }
                else
                {
                    await InsertMasterDataAsync(masterData, conn).ConfigureAwait(false);
                }
            }
        }

        private async Task<bool> MasterDataExistsAsync<T>(Guid rowId, SqlConnection conn)
            where T : IMasterDataObject
        {
            var count = await conn.ExecuteScalarAsync<int>(
                $"SELECT COUNT(1) FROM dbo.{typeof(T).Name} md WHERE md.RowId = @rowId",
                new { rowId }).ConfigureAwait(false);

            return count > 0;
        }

        private async Task InsertMasterDataAsync<T>(T masterDataObject, SqlConnection conn)
            where T : IMasterDataObject
        {
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            var insertUpdateInstructions = _sqlInstructions[typeof(T)];

            await conn.ExecuteAsync(insertUpdateInstructions.InsertSql, transaction: transaction, param: insertUpdateInstructions.InsertParameters(masterDataObject)).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        private async Task UpdateMasterDataObjectAsync<T>(T masterDataObject, SqlConnection conn)
            where T : IMasterDataObject
        {
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);

            var insertUpdateInstructions = _sqlInstructions[typeof(T)];

            await conn.ExecuteAsync(insertUpdateInstructions.UpdateSql, transaction: transaction, param: insertUpdateInstructions.UpdateParameters(masterDataObject)).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        private async Task<SqlConnection> GetConnectionAsync()
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }
    }
}
