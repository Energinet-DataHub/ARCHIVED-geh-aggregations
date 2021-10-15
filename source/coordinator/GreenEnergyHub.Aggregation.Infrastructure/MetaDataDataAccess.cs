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
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
using GreenEnergyHub.Aggregation.Application.Utilities;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums;

namespace GreenEnergyHub.Aggregation.Infrastructure
{
    public class MetaDataDataAccess : IMetaDataDataAccess
    {
        private readonly string _connectionString;

        public MetaDataDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task CreateSnapshotAsync(Snapshot snapshot)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            await InsertSnapshotAsync(snapshot, conn, transaction).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        public async Task CreateJobAsync(Job job)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            await InsertJobAsync(job, conn, transaction).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        public async Task UpdateJobAsync(Job job)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            await UpdateJobAsync(job, conn, transaction).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        public async Task CreateJobResultAsync(JobResult jobResult)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            if (jobResult == null)
            {
                throw new ArgumentNullException(nameof(jobResult));
            }

            await InsertJobResultAsync(jobResult, conn, transaction).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        public async Task UpdateJobResultAsync(JobResult jobResult)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            if (jobResult == null)
            {
                throw new ArgumentNullException(nameof(jobResult));
            }

            await UpdateJobResultAsync(jobResult, conn, transaction).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        public async Task UpdateSnapshotPathAsync(Guid snapshotId, string path)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            if (snapshotId == null)
            {
                throw new ArgumentNullException(nameof(snapshotId));
            }

            const string sql =
                @"UPDATE Snapshot SET
              [Path] = @Path
              WHERE Id = @SnapshotId;";

            await conn.ExecuteAsync(sql, transaction: transaction, param: new { snapshotId, path }).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        public async Task<Job> GetJobAsync(Guid jobId)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);

            var job = await conn
                .QuerySingleAsync<Job>(
                    @"SELECT dbo.Job.* FROM dbo.Job WHERE Job.Id = @JobId;",
                    new { JobId = jobId }).ConfigureAwait(false);

            var snapshot = await conn
                .QuerySingleAsync<Snapshot>(
                    @"SELECT dbo.Snapshot.* FROM dbo.Snapshot WHERE Snapshot.Id = @SnapshotId;",
                    new { SnapshotId = job.SnapshotId }).ConfigureAwait(false);

            job.Snapshot = snapshot;

            return job;
        }

        public async Task<IEnumerable<Result>> GetResultsByTypeAsync(JobTypeEnum type)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);

            var results = await conn
                .QueryAsync<Result>(
                    @"SELECT dbo.Result.* FROM dbo.Result WHERE dbo.Result.Type = @Type AND dbo.Result.DeletedDate IS NULL;",
                    new { Type = type }).ConfigureAwait(false);

            return results;
        }

        private static async Task InsertSnapshotAsync(Snapshot snapshot, SqlConnection conn, DbTransaction transaction)
        {
            const string sql =
                @"INSERT INTO Snapshot ([Id],
                [FromDate],
                [ToDate],
                [CreatedDate],
                [Path],
                [GridAreas]
                ) VALUES
                (@Id, @FromDate, @ToDate, @CreatedDate, @Path, @GridAreas);";

            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                Id = snapshot.Id,
                FromDate = snapshot.FromDate.ToDateTimeUtc(),
                ToDate = snapshot.ToDate.ToDateTimeUtc(),
                CreatedDate = snapshot.CreatedDate.ToDateTimeUtc(),
                Path = snapshot.Path,
                GridAreas = snapshot.GridAreas,
            }).ConfigureAwait(false);
        }

        private static async Task InsertJobAsync(Job job, SqlConnection conn, DbTransaction transaction)
        {
            const string sql =
                @"INSERT INTO Job ([Id],
                [DatabricksJobId],
                [SnapshotId],
                [State],
                [Type],
                [ProcessType],
                [CreatedDate],
                [Owner],
                [ProcessVariant],
                [IsSimulation],
                [Resolution]
                ) VALUES
                (@Id, @DatabricksJobId, @SnapshotId, @State, @Type, @ProcessType, @CreatedDate, @Owner, @ProcessVariant, @IsSimulation, @Resolution);";

            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                job.Id,
                job.DatabricksJobId,
                job.SnapshotId,
                job.State,
                job.Type,
                job.ProcessType,
                job.Owner,
                CreatedDate = job.CreatedDate.ToDateTimeUtc(),
                job.ProcessVariant,
                job.IsSimulation,
                job.Resolution,
            }).ConfigureAwait(false);
        }

        private static async Task UpdateJobAsync(Job job, SqlConnection conn, DbTransaction transaction)
        {
            const string sql =
                @"UPDATE Job SET
              [DatabricksJobId] = @DatabricksJobId,
              [SnapshotId] = @SnapshotId,
              [State] = @State,
              [ProcessType] = @ProcessType,
              [Owner] = @Owner,
              [CompletedDate] = @CompletedDate,
              [ProcessVariant] = @ProcessVariant,
              [IsSimulation] = @IsSimulation
              WHERE Id = @Id;";

            DateTime? completedDate = null;
            if (job.CompletedDate != null)
            {
                completedDate = job.CompletedDate.Value.ToDateTimeUtc();
            }

            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                job.Id,
                job.DatabricksJobId,
                job.SnapshotId,
                job.State,
                job.ProcessType,
                job.Owner,
                CompletedDate = completedDate,
                job.ProcessVariant,
                job.IsSimulation,
            }).ConfigureAwait(false);
        }

        private static async Task InsertJobResultAsync(JobResult jobResult, SqlConnection conn, DbTransaction transaction)
        {
            const string sql =
                @"INSERT INTO JobResult ([JobId], [ResultId], [Path], [State]) VALUES (@JobId, @ResultId, @Path, @State);";

            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                jobResult.JobId,
                jobResult.ResultId,
                jobResult.Path,
                jobResult.State,
            }).ConfigureAwait(false);
        }

        private static async Task UpdateJobResultAsync(JobResult jobResult, SqlConnection conn, DbTransaction transaction)
        {
            const string sql =
                @"UPDATE Result SET [Path] = @Path, [State] = @State WHERE JobId = @JobId AND [ResultId] = @ResultId;";

            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                jobResult.JobId,
                jobResult.ResultId,
                jobResult.Path,
                jobResult.State,
            }).ConfigureAwait(false);
        }

        private async Task<SqlConnection> GetConnectionAsync()
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            return conn;
        }
    }
}
