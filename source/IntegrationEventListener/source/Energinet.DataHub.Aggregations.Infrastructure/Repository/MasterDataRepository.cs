using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Aggregations.Infrastructure.Repository.InsertUpdates;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Infrastructure.Repository
{
    /// <summary>
    /// This represents a reposiory for manipulating IMasterDataObjects
    /// If you want to add a new master data object. Implement a new IInsertUpdate and add it to the _insertUpdate dictonary.
    /// The repository will take care of the rest
    /// </summary>
    public class MasterDataRepository : IMasterDataRepository
    {
        private readonly string _connectionString;
        private readonly IDictionary<Type, IInsertUpdate<IMasterDataObject>> _insertUpdates;

        public MasterDataRepository(string connectionString)
        {
            _connectionString = connectionString;
            _insertUpdates = new Dictionary<Type, IInsertUpdate<IMasterDataObject>>();

            // Insert new master data manipulations to the list here
            _insertUpdates.Add(typeof(MeteringPoint), new MeteringPointInsertUpdate<MeteringPoint>());
        }

        public async Task<List<T>> GetByIdAndDateAsync<T>(string id, Instant effectiveDate)
            where T : IMasterDataObject
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);

            var results = await conn
                .QueryAsync<T>(
                    $"SELECT md.* FROM dbo.{typeof(T).Name} md WHERE md.Id = @id AND md.ToDate > @effectiveDate;",
                    new { id, effectiveDate }).ConfigureAwait(false);

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
            var insertUpdateInstructions = _insertUpdates[typeof(T)];

            await conn.ExecuteAsync(insertUpdateInstructions.InsertSql, transaction: transaction, param: insertUpdateInstructions.InsertParameters(masterDataObject)).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        private async Task UpdateMasterDataObjectAsync<T>(T masterDataObject, SqlConnection conn)
            where T : IMasterDataObject
        {
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);

            var insertUpdateInstructions = _insertUpdates[typeof(T)];

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
