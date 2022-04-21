// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
