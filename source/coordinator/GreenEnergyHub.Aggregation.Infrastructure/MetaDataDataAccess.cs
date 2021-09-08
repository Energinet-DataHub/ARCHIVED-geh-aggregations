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
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
using GreenEnergyHub.Aggregation.Application.Utilities;
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

        public async Task CreateJobAsync(JobMetadata jobMetadata)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            if (jobMetadata == null)
            {
                throw new ArgumentNullException(nameof(jobMetadata));
            }

            await InsertJobAsync(jobMetadata, conn, transaction).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        public async Task UpdateJobAsync(JobMetadata jobMetadata)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            if (jobMetadata == null)
            {
                throw new ArgumentNullException(nameof(jobMetadata));
            }

            await UpdateJobAsync(jobMetadata, conn, transaction).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        public async Task CreateResultItemAsync(Result result)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            await InsertResultItemAsync(result, conn, transaction).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }

        public async Task UpdateResultItemAsync(Result result)
        {
            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            await UpdateResultItemAsync(result, conn, transaction).ConfigureAwait(false);
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

        private static async Task InsertJobAsync(JobMetadata jobMetadata, SqlConnection conn, DbTransaction transaction)
        {
            const string sql =
                @"INSERT INTO Job ([Id],
                [DatabricksJobId],
                [SnapshotId],
                [State],
                [JobType],
                [ProcessType],
                [CreatedDate],
                [Owner],
                [ProcessVariant]
                ) VALUES
                (@Id, @DatabricksJobId, @SnapshotId, @State, @JobType, @ProcessType, @CreatedDate, @Owner, @ProcessVariant);";

            var stateDescription = jobMetadata.State.GetDescription();
            var processTypeDescription = jobMetadata.ProcessType.GetDescription();
            var jobTypeDescription = jobMetadata.JobType.GetDescription();

            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                jobMetadata.Id,
                jobMetadata.DatabricksJobId,
                jobMetadata.SnapshotId,
                State = stateDescription,
                JobType = jobTypeDescription,
                ProcessType = processTypeDescription,
                jobMetadata.Owner,
                CreatedDate = jobMetadata.CreatedDate.ToDateTimeUtc(),
                jobMetadata.ProcessVariant,
            }).ConfigureAwait(false);
        }

        private static async Task UpdateJobAsync(JobMetadata jobMetadata, SqlConnection conn, DbTransaction transaction)
        {
            const string sql =
                @"UPDATE Job SET
              [DatabricksJobId] = @DatabricksJobId,
              [SnapshotId] = @SnapshotId,
              [State] = @State,
              [ProcessType] = @ProcessType
              [Owner] = @Owner,
              [ExecutionEndDate] = @ExecutionEndDate,
              [ProcessVariant] = @ProcessVariant,
              WHERE Id = @Id;";
            var stateDescription = jobMetadata.State.GetDescription();
            var processTypeDescription = jobMetadata.ProcessType.GetDescription();
            DateTime? executionEndDate = null;
            if (jobMetadata.ExecutionEndDate != null)
            {
                executionEndDate = jobMetadata.ExecutionEndDate.Value.ToDateTimeUtc();
            }

            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                jobMetadata.Id,
                jobMetadata.DatabricksJobId,
                jobMetadata.SnapshotId,
                State = stateDescription,
                jobMetadata.ProcessType,
                jobMetadata.Owner,
                ExecutionEndDate = executionEndDate,
                jobMetadata.ProcessVariant,
            }).ConfigureAwait(false);
        }

        private static async Task InsertResultItemAsync(Result result, SqlConnection conn, DbTransaction transaction)
        {
            const string sql =
                @"INSERT INTO Result ([JobId], [Name], [Path], [State]) VALUES (@JobId, @Name, @Path, @State);";

            var resultStateDescription = result.State.GetDescription();
            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                result.JobId,
                result.Name,
                result.Path,
                State = resultStateDescription,
            }).ConfigureAwait(false);
        }

        private static async Task UpdateResultItemAsync(Result result, SqlConnection conn, DbTransaction transaction)
        {
            const string sql =
                @"UPDATE Result SET [Path] = @Path, [State] = @State WHERE JobId = @JobId AND [NAME] = @Name;";

            var resultStateDescription = result.State.GetDescription();
            await conn.ExecuteAsync(sql, transaction: transaction, param: new
            {
                result.JobId,
                result.Name,
                result.Path,
                State = resultStateDescription,
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
