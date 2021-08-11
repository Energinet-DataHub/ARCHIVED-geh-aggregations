using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public enum JobTypeEnum
    {
        [Description("Simulation")]
        Simulation = 0,
        [Description("Live")]
        Live = 1,
    }
}
