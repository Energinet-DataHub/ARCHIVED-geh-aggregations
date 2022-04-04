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
using Energinet.DataHub.Aggregation.Coordinator.Domain.Types;
using NodaTime;

namespace Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator
{
    /// <summary>
    /// This defines the interface for a dispatch strategy
    /// </summary>
    public interface IDispatchStrategy
    {
        /// <summary>
        /// The name of the strategy. Should match the file name of the result in the databricks aggregation
        /// for example: flex_consumption_df.json.snappy is matched by flex_consumption_df
        /// </summary>
        string FriendlyNameInstance { get; }

        /// <summary>
        /// How should the strategy dispatch?
        /// </summary>
        Task DispatchAsync(Stream blobStream, string processType, Instant startTime, Instant endTime, string type, CancellationToken cancellationToken);
    }
}
