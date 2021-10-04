using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces
{
    /// <summary>
    /// This interface abstracts the engine that takes care of creating and running a calculation job on 
    /// </summary>
    public interface ICalculationEngine
    {
        /// <summary>
        /// Does what it says on the tin
        /// </summary>
        /// <param name="jobMetadata"></param>
        /// <param name="processType"></param>
        /// <param name="parameters"></param>
        /// <param name="coordinatorSettingsDataPreparationPythonFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Async task</returns>
        Task CreateAndRunCalculationJobAsync(
            JobMetadata jobMetadata,
            JobProcessTypeEnum processType,
            List<string> parameters,
            string coordinatorSettingsDataPreparationPythonFile,
            CancellationToken cancellationToken);
    }
}
