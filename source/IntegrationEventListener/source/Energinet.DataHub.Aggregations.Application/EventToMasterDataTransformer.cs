using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
