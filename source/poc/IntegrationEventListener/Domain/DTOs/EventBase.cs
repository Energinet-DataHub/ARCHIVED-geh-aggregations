using System;
using System.Collections.Generic;

namespace Domain.DTOs
{
    public abstract class EventBase : IEvent
    {
        public abstract DateTime EffectiveDate { get; set; }

        public abstract string Id { get; set; }

        public List<IReplayableObject> GetObjectsAfterMutate(List<IReplayableObject> replayableObjects, DateTime effectiveDate)
        {
            var returnList = new List<IReplayableObject>();

            foreach (var current in replayableObjects)
            {
                if (current.FromDate >= effectiveDate)
                {
                    Mutate(current);
                    returnList.Add(current);
                    continue;
                }

                if (current.FromDate < effectiveDate && effectiveDate < current.ToDate)
                {
                    var oldValidToDate = current.ToDate;
                    current.ToDate = effectiveDate;

                    var newPeriod = current.ShallowCopy();
                    newPeriod.FromDate = effectiveDate;
                    newPeriod.ToDate = oldValidToDate;
                    newPeriod.RowId = Guid.NewGuid();
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
