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
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.DTOs.MeteringPoints;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Aggregations.Domain.MeteringPoints;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents.Mutators
{
    public class SettlementMethodChangedMutator : MutatorBase
    {
        private readonly SettlementMethodChangedEvent _event;

        public SettlementMethodChangedMutator(SettlementMethodChangedEvent @event)
        {
            _event = @event;
        }

        public override Instant EffectiveDate => _event.EffectiveDate;

        public override string Id => _event.MeteringPointId;

        public override void Mutate(IMasterDataObject masterDataObject)
        {
            if (masterDataObject == null)
            {
                throw new ArgumentNullException(nameof(masterDataObject));
            }

            var meteringPoint = (MeteringPoint)masterDataObject;
            meteringPoint.SettlementMethod = _event.SettlementMethod;
        }
    }
}
