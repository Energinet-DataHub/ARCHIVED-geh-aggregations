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
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents.Mutators
{
    public abstract class MutatorBase : IMasterDataMutator
    {
        public abstract Instant EffectiveDate { get; }

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
