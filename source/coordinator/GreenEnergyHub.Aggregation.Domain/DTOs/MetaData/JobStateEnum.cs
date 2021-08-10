using System.ComponentModel;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public enum JobStateEnum
    {
        [Description("JobMetadata Created")]
        JobCreated = 0,
        [Description("Checking cluster")]
        ClusterCreated = 1,
        [Description("Cluster Warming up")]
        ClusterWarmingUp = 2,
        [Description("Cluster failed to start")]
        ClusterFailed = 3,
        [Description("Calculation running")]
        Calculating = 4,
        [Description("Calculation completed")]
        Completed = 5,
    }
}
