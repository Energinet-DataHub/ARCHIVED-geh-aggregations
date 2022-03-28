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
