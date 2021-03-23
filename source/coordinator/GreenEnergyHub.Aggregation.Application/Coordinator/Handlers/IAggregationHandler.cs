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
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.HourlyConsumption
{
    /// <summary>
    /// This interface dictates how an aggregation handler should be implemented
    /// </summary>
    public interface IAggregationHandler
    {
        /// <summary>
        /// Returns a set of messages for sending outbound for the associated aggregation handler
        /// </summary>
        /// <param name="result"></param>
        /// <param name="processType"></param>
        /// <param name="timeIntervalStart"></param>
        /// <param name="timeIntervalEnd"></param>
        /// <returns>A list of outbound messages</returns>
        IEnumerable<IOutboundMessage> PrepareMessages(
            List<string> result,
            ProcessType processType,
            string timeIntervalStart,
            string timeIntervalEnd);
    }
}
