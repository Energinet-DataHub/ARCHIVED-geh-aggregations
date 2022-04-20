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
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using Energinet.DataHub.Core.Messaging.Transport;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents.MeteringPoints
{
    public record MeteringPointCreatedEvent(
            string MeteringPointId,
            MeteringPointType MeteringPointType,
            string GridArea,
            SettlementMethod? SettlementMethod,
            MeteringMethod MeteringMethod,
            Resolution Resolution,
            Product Product,
            ConnectionState ConnectionState,
            Unit Unit,
            Instant EffectiveDate)
        : IInboundMessage, ITransformingEvent
    {
        public Transaction Transaction { get; set; }

        public string Id => MeteringPointId;

        public List<T> GetObjectsAfterMutate<T>(List<T> replayableObjects, Instant effectiveDate)
            where T : IMasterDataObject
        {
            if (replayableObjects == null)
            {
                throw new ArgumentNullException(nameof(replayableObjects));
            }

            var mp = new MeteringPoint
            {
                RowId = Guid.NewGuid(),
                MeteringPointType = MeteringPointType,
                SettlementMethod = SettlementMethod,
                ConnectionState = ConnectionState,
                Id = MeteringPointId,
                Unit = Unit,
                GridArea = GridArea,
                MeteringMethod = MeteringMethod,
                Resolution = Resolution,
                FromDate = EffectiveDate,
                ToDate = Instant.MaxValue,
            };

            replayableObjects.Add((T)Convert.ChangeType(mp, typeof(T), CultureInfo.InvariantCulture));
            return replayableObjects;
        }
    }
}
