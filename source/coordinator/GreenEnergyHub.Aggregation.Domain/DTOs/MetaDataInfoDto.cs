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

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MetaDataInfoDto
    {
        public MetaDataInfoDto(Guid jobId, Guid snapshotId, Guid resultId, string resultName, string resultPath)
        {
            JobId = jobId;
            SnapshotId = snapshotId;
            ResultId = resultId;
            ResultName = resultName;
            ResultPath = resultPath;
        }

        public Guid JobId { get; set; }

        public Guid SnapshotId { get; set; }

        public Guid ResultId { get; set; }

        public string ResultName { get; set; } = string.Empty;

        public string ResultPath { get; set; } = string.Empty;
    }
}
