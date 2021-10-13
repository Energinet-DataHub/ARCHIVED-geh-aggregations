﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public class Job
    {
        public Job() { }

        public Job(Guid id, Guid snapshotId, JobTypeEnum type, JobStateEnum state, string owner, JobProcessTypeEnum? processType = null, bool isSimulation = false, JobProcessVariantEnum? processVariant = null)
        {
            Id = id;
            Owner = owner;
            SnapshotId = snapshotId;
            Type = type;
            CreatedDate = SystemClock.Instance.GetCurrentInstant();
            ProcessType = processType;
            State = state;
            IsSimulation = isSimulation;
            Owner = owner;
            ProcessVariant = processVariant;
        }

        /// <summary>
        /// Unique id of this job. This is set by the external caller
        /// </summary>
        public Guid Id { get;  }

        /// <summary>
        /// The Databrick Job Id returned from databricks when the job is initiated
        /// </summary>
        public long? DatabricksJobId { get; set; }

        public Guid SnapshotId { get; set; }

        /// <summary>
        /// The state of this run
        /// </summary>
        public JobStateEnum State { get; set; }

        /// <summary>
        /// The job type ie. Preparation, Aggregation, Wholesale
        /// </summary>
        public JobTypeEnum Type { get; }

        /// <summary>
        /// Type of process ie. Aggregation, BalanceFixing, WholesaleFixing, CorrectionSettlement
        /// </summary>
        public JobProcessTypeEnum? ProcessType { get; }

        /// <summary>
        /// Process variant of job ie. FirstRun, SecondRun, ThirdRun
        /// </summary>
        public JobProcessVariantEnum? ProcessVariant { get; set; }

        /// <summary>
        /// A reference to the owner of the job, ie. who started it. Provided by external entity
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// Is job a simulation or not.
        /// </summary>
        public bool IsSimulation { get; set; }

        /// <summary>
        /// The date and time the job was created
        /// </summary>
        public Instant CreatedDate { get; }

        /// <summary>
        /// The date and time the job was started
        /// </summary>
        public Instant? StartedDate { get; set; }

        /// <summary>
        /// The date and time the job was completed
        /// </summary>
        public Instant? CompletedDate { get; set; }

        /// <summary>
        /// The date and time the job was deleted
        /// </summary>
        public Instant? DeletedDate { get; set; }

        public virtual Snapshot Snapshot { get; set; }

        public virtual IEnumerable<JobResult> JobResults { get; set; }
    }
}