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
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public class JobResult
    {
        public JobResult(Guid jobId, Guid resultId, string path, ResultStateEnum state)
        {
            JobId = jobId;
            ResultId = resultId;
            Path = path;
            State = state;
        }

        /// <summary>
        /// Id of job
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// Id of result
        /// </summary>
        public Guid ResultId { get; set; }

        /// <summary>
        /// Path to blob where result is saved
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// State of result ie. NotCompleted, Completed
        /// </summary>
        public ResultStateEnum State { get; set; }

        /// <summary>
        /// Result
        /// </summary>
        public Result Result { get; set; }
    }
}
