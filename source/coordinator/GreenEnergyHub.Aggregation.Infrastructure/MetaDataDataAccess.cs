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

        private static async Task InsertJobAsync(JobMetadata jobMetadata, SqlConnection conn, DbTransaction transaction)
        {
            const string jobSql =
                @"INSERT INTO Jobs ([Id],
                [DatabricksJobId],
                [State],
                [Created],
                [Owner],
                [SnapshotPath],
                [ProcessType],
                [GridArea],
                [ProcessPeriodStart],
                [ProcessPeriodEnd],
                [JobType],
                [ExecutionEnd]) VALUES
                (@Id, @DatabricksJobId, @State, @Created, @Owner, @SnapshotPath, @ProcessType,@GridArea,@ProcessPeriodStart,@ProcessPeriodEnd,@JobType,@ExecutionEnd);";

            var stateDescription = jobMetadata.State.GetDescription();
            var processTypeDescription = jobMetadata.ProcessType.GetDescription();
            var jobTypeDescription = jobMetadata.JobType.GetDescription();

            await conn.ExecuteAsync(jobSql, transaction: transaction, param: new
            {
                jobMetadata.Id,
                jobMetadata.DatabricksJobId,
                State = stateDescription,
                Created = jobMetadata.ExecutionStart.ToDateTimeUtc(),
                Owner = jobMetadata.JobOwner,
                jobMetadata.SnapshotPath,
                ProcessType = processTypeDescription,
                GridArea = jobMetadata.GridArea,
                ProcessPeriodStart = jobMetadata.ProcessPeriod.Start.ToDateTimeUtc(),
                ProcessPeriodEnd = jobMetadata.ProcessPeriod.End.ToDateTimeUtc(),
                JobType = jobTypeDescription,
                ExecutionEnd = jobMetadata.ExecutionEnd.ToDateTimeUtc(),
            }).ConfigureAwait(false);
        }

        private static async Task UpdateJobAsync(JobMetadata jobMetadata, SqlConnection conn, DbTransaction transaction)
        {
            const string jobSql =
                @"UPDATE Jobs SET
              [DatabricksJobId] = @DatabricksJobId,
              [State] = @State,
              [Created] = @Created,
              [ExecutionEnd] = @ExecutionEnd,
              [Owner] = @Owner,
              [SnapshotPath] = @SnapshotPath,
              [ProcessType] = @ProcessType
              WHERE Id = @Id;";
            var stateDescription = jobMetadata.State.GetDescription();
            var processTypeDescription = jobMetadata.ProcessType.GetDescription();
            var jobTypeDescription = jobMetadata.JobType.GetDescription();

            await conn.ExecuteAsync(jobSql, transaction: transaction, param: new
            {
                jobMetadata.Id,
                jobMetadata.DatabricksJobId,
                State = stateDescription,
                Created = jobMetadata.ExecutionStart.ToDateTimeUtc(),
                Owner = jobMetadata.JobOwner,
                jobMetadata.SnapshotPath,
                ProcessType = processTypeDescription,
                ExecutionEnd = jobMetadata.ExecutionEnd.ToDateTimeUtc(),
            }).ConfigureAwait(false);
        }

        private static async Task InsertResultItemAsync(Result result, SqlConnection conn, DbTransaction transaction)
        {
            const string resultItemSql =
                @"INSERT INTO Results ([JobId], [Name], [Path], [State]) VALUES (@JobId, @Name, @Path, @State);";

            var resultStatedescription = result.State.GetDescription();
            await conn.ExecuteAsync(resultItemSql, transaction: transaction, param: new
            {
                result.JobId,
                result.Name,
                result.Path,
                State = resultStatedescription,
            }).ConfigureAwait(false);
        }

        private static async Task UpdateResultItemAsync(Result result, SqlConnection conn, DbTransaction transaction)
        {
            const string resultItemSql =
                @"UPDATE Results SET [Path] = @Path, [State] = @State WHERE JobId = @JobId AND [NAME] = @Name;";

            var resultStatedescription = result.State.GetDescription();
            await conn.ExecuteAsync(resultItemSql, transaction: transaction, param: new
            {
                result.JobId,
                result.Name,
                result.Path,
                State = resultStatedescription,
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
