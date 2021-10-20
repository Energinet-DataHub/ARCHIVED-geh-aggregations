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
using GreenEnergyHub.Aggregation.Domain.DTOs.Metadata;
using GreenEnergyHub.Aggregation.Domain.DTOs.Metadata.Enums;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    /// <summary>
    /// Trigger base arguments
    /// </summary>
    public interface ITriggerBaseArguments
    {
        /// <summary>
        /// Returns arguments used for data preparation databricks job trigger function
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="gridAreas"></param>
        /// <param name="jobId"></param>
        /// <param name="snapshotId"></param>
        /// <returns>List of strings</returns>
        List<string> GetTriggerDataPreparationArguments(Instant fromDate, Instant toDate, string gridAreas, Guid jobId, Guid snapshotId);

        /// <summary>
        /// Returns arguments used for aggregation databricks job trigger function
        /// </summary>
        /// <param name="job"></param>
        /// <returns>List of strings</returns>
        List<string> GetTriggerAggregationArguments(Job job);

        /// <summary>
        /// Returns arguments used for wholesale databricks job trigger function
        /// </summary>
        /// <param name="processType"></param>
        /// <param name="jobId"></param>
        /// <param name="snapshotId"></param>
        /// <returns>List of strings</returns>
        List<string> GetTriggerWholesaleArguments(JobProcessTypeEnum processType, Guid jobId, Guid snapshotId);
    }
}
