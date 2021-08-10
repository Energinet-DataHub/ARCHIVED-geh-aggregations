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
        public JobMetadata(JobProcessTypeEnum processType, Guid id, Interval processPeriod, JobTypeEnum jobType, string jobOwner)
        {
            Id = id;
            ProcessPeriod = processPeriod;
            JobOwner = jobOwner;
            JobType = jobType;
            ExecutionStart = SystemClock.Instance.GetCurrentInstant();
            ProcessType = processType;
            State = JobStateEnum.JobCreated;
            JobOwner = jobOwner;
        }

        /// <summary>
        /// Unique id of this job. This is set by the external caller
        /// </summary>
        public Guid Id { get;  }

        /// <summary>
        /// The grid area we operate on. If null we operate on all grid areas
        /// </summary>
        public string GridArea { get;  }

        /// <summary>
        /// What period are we calculating across ?
        /// </summary>
        public Interval ProcessPeriod { get; }

        /// <summary>
        /// The job type ie. Simulation/Live
        /// </summary>
        public JobTypeEnum JobType { get; }

        /// <summary>
        /// The Databrick JobMetadata Id returned from databricks when the job is initiated
        /// </summary>
        public long DatabricksJobId { get; set; }

        /// <summary>
        /// The state of this run
        /// </summary>
        public JobStateEnum State { get; set; }

        /// <summary>
        /// The time the job was initiated
        /// </summary>
        public Instant ExecutionStart { get; }

        /// <summary>
        /// The time the job was finished
        /// </summary>
        public Instant ExecutionEnd { get; set; }

        /// <summary>
        /// A reference to the owner of the job, ie. who started it. Provided by external entity
        /// </summary>
        public string JobOwner { get; }

        /// <summary>
        /// Where is the snapshot for this calculation stored ?
        /// </summary>
        public string SnapshotPath { get; set; }

        /// <summary>
        /// The type of calculation we are running. for example aggregation, wholesale
        /// </summary>
        public JobProcessTypeEnum ProcessType { get; }

        public string ClusterId { get; set; }

        public long RunId { get; set; }
    }
}
