using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.DTOs;

namespace GreenEnergyHub.Aggregation.Infrastructure
{
    public interface IEventStore
    {
        Task<IEnumerable<IEvent>> LoadStreamAsync(string streamId);

        Task<IEnumerable<IEvent>> LoadStreamAsync(string streamId, int fromVersion);

        Task<bool> AppendToStreamAsync(
            string meteringPointId,
            EventWrapper @eventObject);
    }
}
