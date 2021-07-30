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
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    /// <summary>
    /// Trigger base arguments
    /// </summary>
    public interface ITriggerBaseArguments
    {
        /// <summary>
        /// Returns base arguments used for databricks job trigger functions
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="processType"></param>
        /// <param name="persist"></param>
        /// <returns>List of strings</returns>
        List<string> GetTriggerBaseArguments(Instant beginTime, Instant endTime, string processType, bool persist);
    }
}
