using System;
using System.Collections.Generic;
using Domain.Models;

namespace Domain.DTOs
{
    public class MeteringPointCreatedEvent : IEvent
    {
        public string Id { get; set; }

        public MeteringPointType MeteringPointType { get; set; }

        public SettlementMethod SettlementMethod { get; set; }

        public ConnectionState ConnectionState { get; set; }

        public DateTime EffectiveDate { get; set; }

        public List<IReplayableObject> GetObjectsAfterMutate(List<IReplayableObject> replayableObjects, DateTime effectiveDate)
        {
            if (replayableObjects == null)
            {
                throw new ArgumentNullException(nameof(replayableObjects));
            }

            replayableObjects.Add(new MeteringPoint()
            {
                RowId = Guid.NewGuid(),
                MeteringPointType = MeteringPointType,
                SettlementMethod = SettlementMethod,
                ConnectionState = ConnectionState,
                Id = Id,
                FromDate = EffectiveDate,
                ToDate = DateTime.MaxValue
            });
            return replayableObjects;
        }
    }
}
