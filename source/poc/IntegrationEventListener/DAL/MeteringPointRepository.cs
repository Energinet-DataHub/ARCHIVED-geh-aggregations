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
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.Linq;
using Domain.Models;

namespace DAL
{
    public class MeteringPointRepository : IMeteringPointRepository
    {
        private readonly string _connectionString;

        public MeteringPointRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<MeteringPoint>> GetByIdAndDateAsync(string id, DateTime effectiveDate)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);

            var results = await conn
                .QueryAsync<MeteringPoint>(
                    @"SELECT mp.* FROM dbo.MeteringPoint mp WHERE mp.Id = @id AND mp.ToDate > @effectiveDate;",
                    new { id, effectiveDate }).ConfigureAwait(false);

            return results.ToList();
        }

        public async Task AddOrUpdateMeteringPoints(List<MeteringPoint> meteringPoints)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);

            foreach (var meteringPoint in meteringPoints)
            {
                if (await MeteringPointExists(meteringPoint.RowId, conn).ConfigureAwait(false))
                {
                    await UpdateMeteringPoint(meteringPoint, conn).ConfigureAwait(false);
                }
                else
                {
                    await InsertMeteringPoint(meteringPoint, conn).ConfigureAwait(false);
                }
            }
        }

        private async Task<bool> MeteringPointExists(Guid rowId, SqlConnection conn)
        {
            var count = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM dbo.MeteringPoint mp WHERE mp.RowId = @rowId",
                new { rowId }
            ).ConfigureAwait(false);

            return count > 0;
        }

        private async Task InsertMeteringPoint(MeteringPoint meteringPoint, SqlConnection conn)
        {
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            const string sql =
                @"INSERT INTO dbo.MeteringPoint (RowId, Id, ConnectionState, MeteringPointType, SettlementMethod, FromDate, ToDate)
                VALUES (@RowId, @Id, @ConnectionState, @MeteringPointType, @SettlementMethod, @FromDate, @ToDate)";

            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                meteringPoint.RowId,
                meteringPoint.Id,
                meteringPoint.ConnectionState,
                meteringPoint.SettlementMethod,
                meteringPoint.MeteringPointType,
                meteringPoint.FromDate,
                meteringPoint.ToDate
            }).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        private async Task UpdateMeteringPoint(MeteringPoint meteringPoint, SqlConnection conn)
        {
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            const string sql =
                @"UPDATE MeteringPoint SET
                    [ConnectionState] = @ConnectionState,
                    [SettlementMethod] = @SettlementMethod,
                    [MeteringPointType] = @MeteringPointType,
                    [FromDate] = @FromDate,
                    [ToDate] = @ToDate
                    WHERE RowId = @RowId;";

            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                meteringPoint.RowId,
                meteringPoint.ConnectionState,
                meteringPoint.SettlementMethod,
                meteringPoint.MeteringPointType,
                meteringPoint.FromDate,
                meteringPoint.ToDate
            }).ConfigureAwait(false);
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
