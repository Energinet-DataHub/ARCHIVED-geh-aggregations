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
using Energinet.DataHub.Aggregations.Domain.MasterData;

namespace Energinet.DataHub.Aggregations.Domain
{
    /// <summary>
    /// This takes care of transforming the master data based on an ITransformingEvent
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
