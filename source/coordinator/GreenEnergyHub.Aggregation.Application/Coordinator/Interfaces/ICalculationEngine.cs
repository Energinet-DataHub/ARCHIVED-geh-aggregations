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
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;

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
        /// <param name="job"></param>
        /// <param name="parameters"></param>
        /// <param name="coordinatorSettingsDataPreparationPythonFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Async task</returns>
        Task CreateAndRunCalculationJobAsync(
            Job job,
            List<string> parameters,
            string coordinatorSettingsDataPreparationPythonFile,
            CancellationToken cancellationToken);
    }
}
