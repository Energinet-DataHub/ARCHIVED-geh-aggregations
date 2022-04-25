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
using System.Globalization;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.DTOs.MeteringPoints;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Aggregations.Domain.MasterData.MeteringPoint;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents.Mutators
{
    public class MeteringPointCreatedMutator : IMasterDataMutator
    {
        private readonly MeteringPointCreatedEvent _event;

        public MeteringPointCreatedMutator(MeteringPointCreatedEvent @event)
        {
            _event = @event;
        }

        public Instant EffectiveDate => _event.EffectiveDate;

        public string Id => _event.MeteringPointId;

        //Since we create act directly on the empty list
        public List<T> GetObjectsAfterMutate<T>(List<T> replayableObjects, Instant effectiveDate)
            where T : IMasterDataObject
        {
            if (replayableObjects == null)
            {
                throw new ArgumentNullException(nameof(replayableObjects));
            }

            var mp = new MeteringPoint()
            {
                RowId = Guid.NewGuid(),
                MeteringPointType = _event.MeteringPointType,
                SettlementMethod = _event.SettlementMethod,
                ConnectionState = _event.ConnectionState,
                MeteringPointId = _event.MeteringPointId,
                Unit = _event.Unit,
                GridArea = _event.GridArea,
                MeteringMethod = _event.MeteringMethod,
                Resolution = _event.Resolution,
                Product = _event.Product,
                FromDate = EffectiveDate,
                ToDate = Instant.MaxValue,
            };

            replayableObjects.Add((T)Convert.ChangeType(mp, typeof(T), CultureInfo.InvariantCulture));
            return replayableObjects;
        }
    }
}
