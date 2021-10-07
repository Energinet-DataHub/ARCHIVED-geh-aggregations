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
using System.Threading;
using System.Threading.Tasks;

namespace Energinet.DataHub.Aggregations.Infrastructure
{
    /// <summary>
    /// This provides an interface for the event hub
    /// </summary>
    public interface IEventHubService : IAsyncDisposable
    {
        /// <summary>
        /// This sends a message onto the eventhub
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        Task SendEventHubMessageAsync(string message, CancellationToken cancellationToken = default);
    }
}
