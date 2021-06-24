using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    /// <summary>
    /// This interface specifies an event that represent a change of an object
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Date from which this change is valid from
        /// </summary>
        public Instant EffectuationDate { get; set; }

        /// <summary>
        /// Id of the object to mutate
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// A function to replay this current event on a replayable event
        /// </summary>
        /// <param name="replayableObjects"></param>
        /// <returns>an updated list with the current set of replayed events after this event has mutated the list</returns>
        List<IReplayableObject> GetObjectsAfterMutate(List<IReplayableObject> replayableObjects, Instant effectuationDate);
    }
}
