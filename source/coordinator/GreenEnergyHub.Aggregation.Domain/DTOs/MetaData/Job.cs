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
    public class Job
    {
        public Job(string processType)
        {
            Id = Guid.NewGuid();
            Created = SystemClock.Instance.GetCurrentInstant();
            ProcessType = processType;
            State = "Job created";
            Owner = "Owner"; // TODO: Fill out owner
        }

        public Guid Id { get; set; }

        public long DatabricksJobId { get; set; }

        public string State { get; set; }

        public Instant Created { get; }

        public string Owner { get; set; }

        public string SnapshotPath { get; set; }

        public string ProcessType { get; }

        public string ClusterId { get; set; }

        public long RunId { get; set; }
    }
}
