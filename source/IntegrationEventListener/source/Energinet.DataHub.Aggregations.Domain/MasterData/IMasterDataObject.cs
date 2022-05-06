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
using NodaTime;

namespace Energinet.DataHub.Aggregations.Domain.MasterData
{
    /// <summary>
    /// This interface represents an master data object that can participate in the replay functionality
    /// </summary>
    public interface IMasterDataObject
    {
        /// <summary>
        /// Valid from date
        /// </summary>
        Instant FromDate { get; set; }

        /// <summary>
        /// Valid to date
        /// </summary>
        Instant ToDate { get; set; }

        /// <summary>
        /// Id from DB
        /// </summary>
        Guid? RowId { get; set; }

        /// <summary>
        /// Creates a shallow copy / clone of this object
        /// </summary>
        /// <returns>a clone of the object</returns>
        T ShallowCopy<T>()
            where T : IMasterDataObject;
    }
}
