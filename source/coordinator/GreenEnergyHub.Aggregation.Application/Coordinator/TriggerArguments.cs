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
using System.Linq;
using System.Text.Json;
using GreenEnergyHub.Aggregation.Application.Utilities;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    public class TriggerArguments : ITriggerBaseArguments
    {
        private readonly CoordinatorSettings _coordinatorSettings;

        public TriggerArguments(CoordinatorSettings coordinatorSettings)
        {
            _coordinatorSettings = coordinatorSettings;
        }

        public List<string> GetTriggerDataPreparationArguments(Instant fromDate, Instant toDate, string gridAreas, Guid jobId, Guid snapshotId)
        {
            var args = GetTriggerBaseArguments(jobId, snapshotId);

            var prepArgs = new List<string>
            {
                $"--time-series-path={_coordinatorSettings.TimeSeriesPath}",
                $"--beginning-date-time={fromDate.ToIso8601GeneralString()}",
                $"--end-date-time={toDate.ToIso8601GeneralString()}",
                $"--grid-area={gridAreas}",
            };

            args.AddRange(prepArgs);
            return args;
        }

        public List<string> GetTriggerAggregationArguments(Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            var args = GetTriggerBaseArguments(job.Id, job.SnapshotId);

            var dictionary = JsonSerializer.Serialize(CreateMetaDataDictionary(job));

            var aggregationArgs = new List<string>
            {
                $"--resolution={job.Resolution}",
                $"--process-type={job.ProcessType}",
                $"--meta-data-dictionary={dictionary}",
            };

            args.AddRange(aggregationArgs);
            return args;
        }

        public List<string> GetTriggerWholesaleArguments(JobProcessTypeEnum processType, Guid jobId, Guid snapshotId)
        {
            var args = GetTriggerBaseArguments(jobId, snapshotId);

            var aggregationArgs = new List<string>
            {
                $"--process-type={processType}",
            };

            args.AddRange(aggregationArgs);
            return args;
        }

        private static Dictionary<int, MetaDataInfoDto> CreateMetaDataDictionary(Job job)
        {
            var dict = new Dictionary<int, MetaDataInfoDto>();

            foreach (var jobResult in job.JobResults.OrderBy(x => x.Result.Order))
            {
                var obj = new MetaDataInfoDto(job.Id, job.SnapshotId, jobResult.ResultId, jobResult.Result.Name, jobResult.Path);
                dict.Add(jobResult.Result.Order, obj);
            }

            return dict;
        }

        private List<string> GetTriggerBaseArguments(Guid jobId, Guid snapshotId)
        {
            return new List<string>
            {
                $"--data-storage-account-name={_coordinatorSettings.DataStorageAccountName}",
                $"--data-storage-account-key={_coordinatorSettings.DataStorageAccountKey}",
                $"--data-storage-container-name={_coordinatorSettings.DataStorageContainerName}",
                $"--result-url={_coordinatorSettings.ResultUrl}?code={_coordinatorSettings.HostKey}",
                $"--snapshot-url={_coordinatorSettings.SnapshotUrl}?code={_coordinatorSettings.HostKey}",
                $"--persist-source-dataframe-location={_coordinatorSettings.PersistLocation}",
                $"--job-id={jobId}",
                $"--snapshot-id={snapshotId}",
            };
        }
    }
}
