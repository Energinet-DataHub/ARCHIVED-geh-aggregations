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
using GreenEnergyHub.Aggregation.Domain;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces
{
    /// <summary>
    /// Dispatches messages out of the application
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Utility method for deserializing with custom nodatimehandling
        /// </summary>
        /// <typeparam name="T">The T to deserialize to</typeparam>
        /// <param name="str"></param>
        /// <returns>Deserialized object</returns>
        public T Deserialize<T>(string str);

        /// <summary>
        /// Send a <paramref name="message"/> to a channel
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="type">Type of message</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <exception cref="ArgumentNullException">message is <c>null</c></exception>
        public Task DispatchAsync<T>(T message, string type, CancellationToken cancellationToken)
            where T : IOutboundMessage;
    }
}
