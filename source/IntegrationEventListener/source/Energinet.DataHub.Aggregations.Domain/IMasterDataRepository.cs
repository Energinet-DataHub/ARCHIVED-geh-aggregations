using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Domain
{
    /// <summary>
    /// This represents the storage that can retrieve and add/update master data objects
    /// </summary>
    public interface IMasterDataRepository
    {
        /// <summary>
        /// Get the entire list of master data objects with the type T and provided id where the effective date is larger than effectiveDate
        /// </summary>
        /// <typeparam name="T">The IMasterDataObject type</typeparam>
        /// <param name="id">Id of the master data object</param>
        /// <param name="effectiveDate">Find objects newer than this</param>
        /// <returns>List of T</returns>
        Task<List<T>> GetByIdAndDateAsync<T>(string id, Instant effectiveDate)
            where T : IMasterDataObject;

        /// <summary>
        /// Adds or updates a list of master data objects
        /// </summary>
        /// <typeparam name="T">The IMasterDataObject type</typeparam>
        /// <param name="masterDataObjects">List of IMasterDataObjects</param>
        /// <returns>Async task</returns>
        Task AddOrUpdateMeteringPointsAsync<T>(List<T> masterDataObjects)
            where T : IMasterDataObject;
    }
}
