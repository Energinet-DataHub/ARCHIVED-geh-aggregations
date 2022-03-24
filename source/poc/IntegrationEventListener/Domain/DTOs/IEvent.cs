using System;
using System.Collections.Generic;

namespace Domain.DTOs
{
    /// <summary>
    /// This interface specifies an event that represent a change of an object
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Date from which this change is valid from
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Id of the object to mutate
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// A function to replay this current event on a replayable event
        /// </summary>
        /// <param name="replayableObjects"></param>
        /// <returns>an updated list with the current set of replayed events after this event has mutated the list</returns>
        List<IReplayableObject> GetObjectsAfterMutate(List<IReplayableObject> replayableObjects, DateTime effectuationDate);
    }
}
