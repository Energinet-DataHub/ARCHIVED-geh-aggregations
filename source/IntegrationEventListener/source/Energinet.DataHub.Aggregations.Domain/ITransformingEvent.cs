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

using System.Collections.Generic;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Domain
{
    /// <summary>
    /// This interface specifies an event that represent a change of an object
    /// </summary>
    public interface ITransformingEvent
    {
        /// <summary>
        /// Date from which this change is valid from
        /// </summary>
        public Instant EffectiveDate { get; }

        /// <summary>
        /// The id of the master data object
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// A function to replay this current event on a replayable event
        /// </summary>
        /// <param name="replayableObjects"></param>
        /// <returns>an updated list with the current set of replayed events after this event has mutated the list</returns>
        List<T> GetObjectsAfterMutate<T>(List<T> replayableObjects, Instant effectiveDate)
            where T : IMasterDataObject;
    }
}
