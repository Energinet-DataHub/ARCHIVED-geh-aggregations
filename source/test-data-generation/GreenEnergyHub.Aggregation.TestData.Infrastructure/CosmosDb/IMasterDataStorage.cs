using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb
{
    /// <summary>
    /// An interface to the location where the master data is stored
    /// </summary>
    public interface IMasterDataStorage
    {
        /// <summary>
        /// Writes a metering point to the storage
        /// </summary>
        /// <param name="mp"></param>
        /// <returns>Task</returns>
        Task WriteMeteringPointAsync(MeteringPoint mp);
    }
}
