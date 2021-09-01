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
using System.Text;
using GreenEnergyHub.Aggregation.Domain.Types;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public class JobMetadata
    {
        public JobMetadata(Guid id, Guid snapshotId, JobTypeEnum jobType, JobProcessTypeEnum processType, string owner, string processVariant = null)
        {
            Id = id;
            Owner = owner;
            SnapshotId = snapshotId;
            JobType = jobType;
            CreatedDate = SystemClock.Instance.GetCurrentInstant();
            ProcessType = processType;
            Owner = owner;
            ProcessVariant = processVariant;
        }

        /// <summary>
        /// Unique id of this job. This is set by the external caller
        /// </summary>
        public Guid Id { get;  }

        /// <summary>
        /// The Databrick JobMetadata Id returned from databricks when the job is initiated
        /// </summary>
        public long? DatabricksJobId { get; set; }

        public Guid SnapshotId { get; set; }

        /// <summary>
        /// The state of this run
        /// </summary>
        public JobStateEnum State { get; set; }

        /// <summary>
        /// The job type ie. Simulation/Live
        /// </summary>
        public JobTypeEnum JobType { get; }

        /// <summary>
        /// The type of calculation we are running. for example aggregation, wholesale
        /// </summary>
        public JobProcessTypeEnum ProcessType { get; }

        /// <summary>
        /// A reference to the owner of the job, ie. who started it. Provided by external entity
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// The time the job was initiated
        /// </summary>
        public Instant CreatedDate { get; }

        /// <summary>
        /// The time the job was finished
        /// </summary>
        public Instant? ExecutionEndDate { get; set; }

        public string? ProcessVariant { get; }

        public virtual Snapshot Snapshot { get; set; }

        public virtual IEnumerable<Result> Results { get; set; }
    }
}
