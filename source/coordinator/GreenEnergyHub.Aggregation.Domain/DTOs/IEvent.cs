using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public interface IEvent
    {
        List<MeteringPoint> GetObjectsAfterMutate(List<MeteringPoint> meteringPoints);
    }
}
