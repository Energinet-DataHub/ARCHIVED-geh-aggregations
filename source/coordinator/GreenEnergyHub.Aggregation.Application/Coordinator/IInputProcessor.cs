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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.Types;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    /// <summary>
    /// Input processor
    /// </summary>
    public interface IInputProcessor
    {
        /// <summary>
        /// This method processes the input,finds the appropriate dispatch strategy which then dispatches it
        /// </summary>
        /// <param name="nameOfAggregation"></param>
        /// <param name="blobStream"></param>
        /// <param name="processType"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="cancellationToken"></param>
        Task ProcessInputAsync(
            string nameOfAggregation,
            Stream blobStream,
            string processType,
            Instant startTime,
            Instant endTime,
            CancellationToken cancellationToken);
    }
}
