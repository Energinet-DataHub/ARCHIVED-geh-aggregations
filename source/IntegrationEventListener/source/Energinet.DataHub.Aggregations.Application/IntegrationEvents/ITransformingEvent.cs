using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents
{
    /// <summary>
    /// This interface specifies an event that represent a change of an object
    /// </summary>
    public interface ITransformingEvent
    {
        /// <summary>
        /// Date from which this change is valid from
        /// </summary>
        public Instant EffectiveDate { get; }

      /// <summary>
        /// A function to replay this current event on a replayable event
        /// </summary>
        /// <param name="replayableObjects"></param>
        /// <returns>an updated list with the current set of replayed events after this event has mutated the list</returns>
        List<T> GetObjectsAfterMutate<T>(List<T> replayableObjects, Instant effectiveDate)
            where T : IReplayableObject;
    }
}
