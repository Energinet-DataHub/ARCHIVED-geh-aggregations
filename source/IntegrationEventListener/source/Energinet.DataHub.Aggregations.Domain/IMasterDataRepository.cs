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
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Domain
{
    /// <summary>
    /// This represents the storage that can retrieve and add/update master data objects
    /// </summary>
    public interface IMasterDataRepository<T>
        where T : IMasterDataObject
    {
        /// <summary>
        /// Get the entire list of master data objects with the type T and provided id where the effective date is larger than effectiveDate
        /// </summary>
        /// <typeparam name="T">The IMasterDataObject type</typeparam>
        /// <param name="id">Id of the master data object</param>
        /// <param name="effectiveDate">Find objects newer than this</param>
        /// <returns>List of T</returns>
        Task<List<T>> GetByIdAndDateAsync(string id, Instant effectiveDate);

        /// <summary>
        /// Adds or updates a list of master data objects
        /// </summary>
        /// <typeparam name="T">The IMasterDataObject type</typeparam>
        /// <param name="masterDataObjects">List of IMasterDataObjects</param>
        /// <returns>Async task</returns>
        Task AddOrUpdateAsync(List<T> masterDataObjects);
    }
}
