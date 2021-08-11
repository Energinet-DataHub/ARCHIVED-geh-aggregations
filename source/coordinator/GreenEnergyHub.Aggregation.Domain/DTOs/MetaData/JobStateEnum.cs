using System.ComponentModel;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public enum JobStateEnum
    {
        [Description("JobMetadata Created")]
        JobCreated = 0,
        [Description("Creating cluster")]
        ClusterStartup = 1,
        [Description("Cluster created")]
        ClusterCreated = 2,
        [Description("Cluster Warming up")]
        ClusterWarmingUp = 3,
        [Description("Cluster failed to start")]
        ClusterFailed = 4,
        [Description("Calculation running")]
        Calculating = 5,
        [Description("Calculation completed")]
        Completed = 6,
    }
}
