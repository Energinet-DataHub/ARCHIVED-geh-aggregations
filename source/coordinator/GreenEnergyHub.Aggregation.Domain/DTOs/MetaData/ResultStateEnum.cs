using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public enum ResultStateEnum
    {
        [Description("Started")]
        Started = 0,
        [Description("Ready to dispatch")]
        ReadyToDispatch = 1,
        [Description("Dispatched")]
        Dispatched = 2,
        [Description("Stream Captured")]
        StreamCaptured = 3,
    }
}
