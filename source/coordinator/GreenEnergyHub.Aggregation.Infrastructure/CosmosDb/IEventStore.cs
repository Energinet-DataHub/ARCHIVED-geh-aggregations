using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventListener;
using GreenEnergyHub.Aggregation.Domain.DTOs;

namespace GreenEnergyHub.Aggregation.Infrastructure.CosmosDb
{
    public interface IEventStore
    {
        Task<EventStream> LoadStreamAsync(string streamId);

        Task<EventStream> LoadStreamAsync(string streamId, int fromVersion);

        Task<bool> AppendToStreamAsync(
            string meteringPointId,
            object @eventObject);
    }
}
