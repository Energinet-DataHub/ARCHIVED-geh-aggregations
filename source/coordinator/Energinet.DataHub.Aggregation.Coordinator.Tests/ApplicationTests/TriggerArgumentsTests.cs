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
using Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator;
using Energinet.DataHub.Aggregation.Coordinator.Application.Utilities;
using Energinet.DataHub.Aggregation.Coordinator.Domain.DTOs.Metadata;
using Energinet.DataHub.Aggregation.Coordinator.Domain.DTOs.Metadata.Enums;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregation.Coordinator.Tests.ApplicationTests
{
    [UnitTest]
    public class TriggerArgumentsTests
    {
        private readonly CoordinatorSettings _coordinatorSettings;
        private readonly ITriggerBaseArguments _sut;

        public TriggerArgumentsTests()
        {
            _coordinatorSettings = new CoordinatorSettings
            {
                DataStorageAccountKey = "DataStorageAccountKey",
                DataStorageAccountName = "DataStorageAccountName",
                DataStorageContainerName = "DataStorageContainerName",
                SharedStorageAccountKey = "SharedStorageAccountKey",
                SharedStorageAccountName = "SharedStorageAccountName",
                SharedStorageAggregationsContainerName = "SharedStorageContainerName",
                MeteringPointsPath = "MeteringPointsPath",
                MarketRolesPath = "MarketRolesPath",
                ChargesPath = "ChargesPath",
                ChargeLinksPath = "ChargeLinksPath",
                ChargePricesPath = "ChargePricesPath",
                EsBrpRelationsPath = "EsBrpRelationsPath",
                GridLossSystemCorrectionPath = "GridLossSystemCorrectionPath",
                AggregationPythonFile = "AggregationPythonFile",
                ClusterTimeoutMinutes = 10,
                ConnectionStringDatabricks = "ConnectionStringDatabricks",
                DataPreparationPythonFile = "DataPreparationPythonFile",
                ResultUrl = new Uri("https://ResultUrl.com"),
                SnapshotsBasePath = "SnapshotPath",
                SnapshotNotifyUrl = new Uri("https://SnapshotNotifyUrl.com"),
                TimeSeriesPath = "TimeSeriesPath",
                TokenDatabricks = "TokenDatabricks",
                WholesalePythonFile = "WholesalePythonFile",
            };

            _sut = new TriggerArguments(_coordinatorSettings);
        }

        [Fact]
        public void Test_GetTriggerDataPreparationArguments_ReturnsCorrectArguments()
        {
            var fromDate = Instant.FromUtc(2020, 1, 1, 0, 0);
            var toDate = Instant.FromUtc(2020, 2, 1, 0, 0);
            var jobId = Guid.NewGuid();
            var snapshotId = Guid.NewGuid();

            var args = _sut.GetTriggerDataPreparationArguments(fromDate, toDate, string.Empty, jobId, snapshotId);

            Assert.Contains($"--time-series-path={_coordinatorSettings.TimeSeriesPath}", args);
            Assert.Contains($"--metering-points-path={_coordinatorSettings.MeteringPointsPath}", args);
            Assert.Contains($"--market-roles-path={_coordinatorSettings.MarketRolesPath}", args);
            Assert.Contains($"--charges-path={_coordinatorSettings.ChargesPath}", args);
            Assert.Contains($"--charge-links-path={_coordinatorSettings.ChargeLinksPath}", args);
            Assert.Contains($"--charge-prices-path={_coordinatorSettings.ChargePricesPath}", args);
            Assert.Contains($"--es-brp-relations-path={_coordinatorSettings.EsBrpRelationsPath}", args);
            Assert.Contains($"--grid-loss-system-correction-path={_coordinatorSettings.GridLossSystemCorrectionPath}", args);
            Assert.Contains($"--beginning-date-time={fromDate.ToIso8601GeneralString()}", args);
            Assert.Contains($"--end-date-time={toDate.ToIso8601GeneralString()}", args);
            Assert.Contains($"--grid-area={string.Empty}", args);
            Assert.Contains($"--shared-storage-account-name={_coordinatorSettings.SharedStorageAccountName}", args);
            Assert.Contains($"--shared-storage-account-key={_coordinatorSettings.SharedStorageAccountKey}", args);
            Assert.Contains($"--shared-storage-aggregations-container-name={_coordinatorSettings.SharedStorageAggregationsContainerName}", args);
            Assert.Contains($"--shared-storage-time-series-container-name={_coordinatorSettings.SharedStorageTimeSeriesContainerName}", args);
            AssertBaseArguments(args, jobId, snapshotId);
        }

        [Fact]
        public void Test_GetTriggerAggregationArguments_ReturnsCorrectArguments()
        {
            var jobId = Guid.NewGuid();
            var snapshotId = Guid.NewGuid();

            var job = new Job(
                jobId,
                snapshotId,
                JobTypeEnum.Aggregation,
                JobStateEnum.Started,
                "test_user",
                ResolutionEnum.Hour,
                JobProcessTypeEnum.Aggregation,
                false);

            var args = _sut.GetTriggerAggregationArguments(job);

            Assert.Contains($"--process-type={job.ProcessType}", args);
            AssertBaseArguments(args, jobId, snapshotId);
        }

        [Fact]
        public void Test_GetTriggerWholesaleArguments_ReturnsCorrectArguments()
        {
            var jobId = Guid.NewGuid();
            var snapshotId = Guid.NewGuid();
            var processType = JobProcessTypeEnum.WholesaleFixing;

            var args = _sut.GetTriggerWholesaleArguments(processType, jobId, snapshotId);

            Assert.Contains($"--process-type={processType}", args);
            AssertBaseArguments(args, jobId, snapshotId);
        }

        private void AssertBaseArguments(List<string> args, Guid jobId, Guid snapshotId)
        {
            Assert.Contains($"--data-storage-account-name={_coordinatorSettings.DataStorageAccountName}", args);
            Assert.Contains($"--data-storage-account-key={_coordinatorSettings.DataStorageAccountKey}", args);
            Assert.Contains($"--data-storage-container-name={_coordinatorSettings.DataStorageContainerName}", args);
            Assert.Contains($"--result-url={_coordinatorSettings.ResultUrl}", args);
            Assert.Contains($"--snapshot-notify-url={_coordinatorSettings.SnapshotNotifyUrl}", args);
            Assert.Contains($"--snapshot-path={_coordinatorSettings.SnapshotsBasePath}", args);
            Assert.Contains($"--job-id={jobId}", args);
            Assert.Contains($"--snapshot-id={snapshotId}", args);
        }
    }
}
