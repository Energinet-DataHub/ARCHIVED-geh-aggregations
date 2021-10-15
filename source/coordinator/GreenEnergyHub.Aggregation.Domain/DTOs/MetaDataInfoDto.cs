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
