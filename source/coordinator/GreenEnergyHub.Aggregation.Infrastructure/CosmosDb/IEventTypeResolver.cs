using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.Infrastructure.CosmosDb
{
    public interface IEventTypeResolver
    {
        Type GetEventType(string typeName);
    }
}
