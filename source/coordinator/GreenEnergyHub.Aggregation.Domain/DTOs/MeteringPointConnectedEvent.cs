using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MeteringPointConnectedEvent : IEvent
    {
        public MeteringPointConnectedEvent(string meteringPointId)
        {
            MeteringPointId = meteringPointId;
        }

        public string MeteringPointId { get; }

        public bool Connected => true;

        public DateTime EffectuationDate { get; set; }

        public List<MeteringPoint> GetObjectsAfterMutate(List<MeteringPoint> meteringPoints)
        {
            var returnList = new List<MeteringPoint>();

            foreach (var current in meteringPoints)
            {
                if (current.ValidFrom >= EffectuationDate)
                {
                    current.Connected = Connected;
                    returnList.Add(current);
                    continue;
                }

                if (current.ValidFrom < EffectuationDate && EffectuationDate < current.ValidTo)
                {
                    var oldValidToDate = current.ValidTo;
                    current.ValidTo = EffectuationDate;

                    var newPeriod = current.ShallowCopy();
                    newPeriod.ValidFrom = EffectuationDate;
                    newPeriod.ValidTo = oldValidToDate;
                    newPeriod.Connected = Connected;

                    returnList.Add(current);
                    returnList.Add(newPeriod);
                }
            }

            return returnList;
        }
    }
}
