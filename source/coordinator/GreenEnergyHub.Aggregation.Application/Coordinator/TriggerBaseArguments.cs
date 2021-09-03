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
using GreenEnergyHub.Aggregation.Application.Utilities;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    public class TriggerBaseArguments : ITriggerBaseArguments
    {
        private readonly CoordinatorSettings _coordinatorSettings;

        public TriggerBaseArguments(CoordinatorSettings coordinatorSettings)
        {
            _coordinatorSettings = coordinatorSettings;
        }

        public List<string> GetTriggerDataPreparationArguments(Instant fromDate, Instant toDate, string gridAreas, JobProcessTypeEnum processType, Guid jobId, Guid snapshotId)
        {
            var args = GetTriggerBaseArguments(processType, jobId, snapshotId);

            var prepArgs = new List<string>
            {
                $"--time-series-path={_coordinatorSettings.TimeSeriesPath}",
                $"--cosmos-account-endpoint={_coordinatorSettings.CosmosAccountEndpoint}",
                $"--cosmos-account-key={_coordinatorSettings.CosmosAccountKey}",
                $"--cosmos-database={_coordinatorSettings.CosmosDatabase}",
                $"--cosmos-container-metering-points={_coordinatorSettings.CosmosContainerMeteringPoints}",
                $"--cosmos-container-market-roles={_coordinatorSettings.CosmosContainerMarketRoles}",
                $"--cosmos-container-grid-loss-sys-corr={_coordinatorSettings.CosmosContainerGridLossSysCorr}",
                $"--cosmos-container-es-brp-relations={_coordinatorSettings.CosmosContainerEsBrpRelations}",
                $"--beginning-date-time={fromDate.ToIso8601GeneralString()}",
                $"--end-date-time={toDate.ToIso8601GeneralString()}",
                $"--grid-area={gridAreas}",
                $"--cosmos-container-charges={_coordinatorSettings.CosmosContainerCharges}",
                $"--cosmos-container-charge-links={_coordinatorSettings.CosmosContainerChargeLinks}",
                $"--cosmos-container-charge-prices={_coordinatorSettings.CosmosContainerChargePrices}",
            };

            args.AddRange(prepArgs);
            return args;
        }

        public List<string> GetTriggerAggregationArguments(JobProcessTypeEnum processType, Guid jobId, Guid snapshotId, string resolution)
        {
            var args = GetTriggerBaseArguments(processType, jobId, snapshotId);

            var aggregationArgs = new List<string>
            {
                $"--resolution={resolution}",
            };

            args.AddRange(aggregationArgs);
            return args;
        }

        public List<string> GetTriggerWholesaleArguments(JobProcessTypeEnum processType, Guid jobId, Guid snapshotId)
        {
            var args = GetTriggerBaseArguments(processType, jobId, snapshotId);

            return args;
        }

        private List<string> GetTriggerBaseArguments(JobProcessTypeEnum processType, Guid jobId, Guid snapshotId)
        {
            return new List<string>
            {
                $"--data-storage-account-name={_coordinatorSettings.DataStorageAccountName}",
                $"--data-storage-account-key={_coordinatorSettings.DataStorageAccountKey}",
                $"--data-storage-container-name={_coordinatorSettings.DataStorageContainerName}",
                $"--telemetry-instrumentation-key={_coordinatorSettings.TelemetryInstrumentationKey}",
                $"--process-type={processType}",
                $"--result-url={_coordinatorSettings.ResultUrl}?code={_coordinatorSettings.HostKey}",
                $"--snapshot-url={_coordinatorSettings.SnapshotUrl}?code={_coordinatorSettings.HostKey}",
                $"--persist-source-dataframe-location={_coordinatorSettings.PersistLocation}",
                $"--job-id={jobId}",
                $"--snapshot-id={snapshotId}",
            };
        }
    }
}
