using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;

namespace GreenEnergyHub.Aggregation.Infrastructure
{
    public class MetaDataDataAccess : IMetaDataDataAccess
    {
        private readonly string _connectionString;

        public MetaDataDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task CreateJobAsync(Job job)
        {
            using (var conn = await GetConnectionAsync())
            {
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    await InsertJobAsync(job, conn, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task UpdateJobAsync(Job job)
        {
            using (var conn = await GetConnectionAsync())
            {
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    await UpdateJobAsync(job, conn, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task CreateResultItemAsync(string jobId, Result result)
        {
            using (var conn = await GetConnectionAsync())
            {
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    await InsertResultItemAsync(jobId, result, conn, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task UpdateResultItemAsync(Result result)
        {
            using (var conn = await GetConnectionAsync())
            {
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    await UpdateResultItemAsync(result, conn, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        private static async Task InsertJobAsync(Job job, SqlConnection conn, DbTransaction transaction)
        {
            const string jobSql =
                @"INSERT INTO Jobs ([Id], [DatabricksJobId], [State], [Created], [Owner], [SnapshotPath], [ProcessType]) VALUES
                (@Id, @DatabricksJobId, @State, @Created, @Owner, @SnapshotPath, @ProcessType);";

            await conn.ExecuteAsync(jobSql, transaction: transaction, param: new
            {
                job.Id,
                job.DatabricksJobId,
                job.State,
                Created = job.Created.ToDateTimeUtc(),
                job.Owner,
                job.SnapshotPath,
                ProcessType = job.ProcessType.ToString(),
            }).ConfigureAwait(false);
        }

        private static async Task UpdateJobAsync(Job job, SqlConnection conn, DbTransaction transaction)
        {
            const string jobSql =
                @"UPDATE Jobs SET
              [DatabricksJobId] = @DatabricksJobId,
              [State] = @State,
              [Created] = @Created,
              [Owner] = @Owner,
              [SnapshotPath] = @SnapshotPath,
              [ProcessType] = @ProcessType
              WHERE Id = @Id;";

            await conn.ExecuteAsync(jobSql, transaction: transaction, param: new
            {
                job.Id,
                job.DatabricksJobId,
                job.State,
                Created = job.Created.ToDateTimeUtc(),
                job.Owner,
                job.SnapshotPath,
                ProcessType = job.ProcessType.ToString(),
            }).ConfigureAwait(false);
        }

        private async Task InsertResultItemAsync(string jobId, Result result, SqlConnection conn, DbTransaction transaction)
        {
            const string resultItemSql =
                @"INSERT INTO Results ([JobId], [Name], [Path]) VALUES (@JobId, @Name, @Path);";

            await conn.ExecuteAsync(resultItemSql, transaction: transaction, param: new
            {
                JobId = jobId,
                result.Name,
                result.Path,
                result.State, //TODO: Create State in database migration script
            }).ConfigureAwait(false);
        }

        private async Task UpdateResultItemAsync(string jobId, Result result, SqlConnection conn, DbTransaction transaction)
        {
            const string resultItemSql =
                @"UPDATE Results SET [Path] = @Path, [State] = @State WHERE JobId = @JobId AND [NAME] = @Name;";

            await conn.ExecuteAsync(resultItemSql, transaction: transaction, param: new
            {
                JobId = jobId,
                result.Name,
                result.Path,
                result.State,
            }).ConfigureAwait(false);
        }

        private async Task<SqlConnection> GetConnectionAsync()
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }
    }
}
