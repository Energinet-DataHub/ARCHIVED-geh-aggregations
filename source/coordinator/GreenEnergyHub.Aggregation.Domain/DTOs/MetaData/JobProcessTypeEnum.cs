using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public enum JobProcessTypeEnum
    {
        [Description("Aggregation")]
        Aggregation = 0,
        [Description("Wholesale")]
        Wholesale = 1,
    }
}
