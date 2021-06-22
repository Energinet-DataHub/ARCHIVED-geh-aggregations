using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public interface IEvent
    {
        DateTime Timestamp { get; }
    }
}
