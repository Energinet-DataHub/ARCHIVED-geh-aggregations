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
using System.IO;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Serialization
{
    /// <summary>
    /// Contract serialization and deserialization of JSON.
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// Parse the text representing a single JSON value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <returns>A <typeparamref name="TValue"/> representation of the JSON value.</returns>
        /// <param name="json">JSON text to parse.</param>
        TValue Deserialize<TValue>(string json);

        /// <summary>
        /// Parse stream representing multiple JSON value into an enumerable list of <typeparamref name="TValue"/>
        /// </summary>
        /// <returns>A <typeparamref name="TValue"/> representation of the JSON value.</returns>
        /// <param name="stream">Stream to parse.</param>
        IEnumerable<TValue> DeserializeMultipleContent<TValue>(Stream stream);
    }
}
