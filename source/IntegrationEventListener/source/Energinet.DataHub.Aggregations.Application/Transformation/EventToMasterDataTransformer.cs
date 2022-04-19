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

using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;

namespace Energinet.DataHub.Aggregations.Application
{
    public class EventToMasterDataTransformer : IEventToMasterDataTransformer
    {
        private readonly IMasterDataRepository _masterDataRepository;

        public EventToMasterDataTransformer(IMasterDataRepository masterDataRepository)
        {
            _masterDataRepository = masterDataRepository;
        }

        public async Task HandleTransformAsync<TTransformingEvent, TMasterDataObject>(TTransformingEvent evt)
            where TTransformingEvent : ITransformingEvent
            where TMasterDataObject : IMasterDataObject
        {
            var currentMasterDataObjects = await _masterDataRepository.GetByIdAndDateAsync<TMasterDataObject>(evt.Id, evt.EffectiveDate).ConfigureAwait(false);
            var masterDataObjectsAfterMutate = evt.GetObjectsAfterMutate(currentMasterDataObjects, evt.EffectiveDate);
            await _masterDataRepository.AddOrUpdateMeteringPointsAsync(masterDataObjectsAfterMutate).ConfigureAwait(false);
        }
    }
}
