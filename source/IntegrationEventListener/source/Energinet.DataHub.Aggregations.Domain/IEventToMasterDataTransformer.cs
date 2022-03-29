using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain.MasterData;

namespace Energinet.DataHub.Aggregations.Domain
{
    /// <summary>
    /// This takes care of transforming the masterd ata based on an ITransformingEvent
    /// </summary>
    public interface IEventToMasterDataTransformer
    {
        /// <summary>
        /// Handles the transformation of master data based on the T
        /// </summary>
        /// <typeparam name="TTransformingEvent">Type of event that we handle</typeparam>
        /// <typeparam name="TMasterDataObject">Type of master data that we manipulate</typeparam>
        /// <returns>async task</returns>
        public Task HandleTransformAsync<TTransformingEvent, TMasterDataObject>(TTransformingEvent evt)
            where TTransformingEvent : ITransformingEvent
            where TMasterDataObject : IMasterDataObject;
    }
}
