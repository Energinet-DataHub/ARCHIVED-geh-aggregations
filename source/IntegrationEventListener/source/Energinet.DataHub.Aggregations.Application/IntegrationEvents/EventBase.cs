using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents
{
    public abstract record EventBase : ITransformingEvent
    {
        public abstract Instant EffectiveDate { get; init; }

        public abstract string Id { get; }

        public List<T> GetObjectsAfterMutate<T>(List<T> replayableObjects, Instant effectiveDate)
            where T : IMasterDataObject
        {
            var returnList = new List<T>();

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

                    var newPeriod = current.ShallowCopy<T>();
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

        public abstract void Mutate(IMasterDataObject masterDataObject);
    }
}
