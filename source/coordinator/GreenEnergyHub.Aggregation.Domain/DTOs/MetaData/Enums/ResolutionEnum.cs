using System.ComponentModel;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums
{
    public enum ResolutionEnum
    {
        [Description("15 minutes")]
        Quarter = 0,
        [Description("1 hour")]
        Hour = 1,
    }
}
