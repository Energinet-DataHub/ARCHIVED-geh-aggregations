using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public abstract class EventBase<T> : IEvent
        where T : IReplayableObject
    {
        public abstract Instant EffectuationDate { get; set; }

        public abstract string Id { get; }

        public List<IReplayableObject> GetObjectsAfterMutate(List<IReplayableObject> replayableObjects, Instant effectuationDate)
        {
            var returnList = new List<IReplayableObject>();

            foreach (var current in replayableObjects)
            {
                if (current.ValidFrom >= effectuationDate)
                {
                    Mutate(current);
                    returnList.Add(current);
                    continue;
                }

                if (current.ValidFrom < effectuationDate && effectuationDate < current.ValidTo)
                {
                    var oldValidToDate = current.ValidTo;
                    current.ValidTo = effectuationDate;

                    var newPeriod = current.ShallowCopy();
                    newPeriod.ValidFrom = effectuationDate;
                    newPeriod.ValidTo = oldValidToDate;
                    Mutate(newPeriod);

                    returnList.Add(current);
                    returnList.Add(newPeriod);
                    continue;
                }

                returnList.Add(current);
            }

            return returnList;
        }

        public abstract void Mutate(IReplayableObject replayableObject);
    }
}
