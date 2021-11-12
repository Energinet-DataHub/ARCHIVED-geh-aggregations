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
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Serialization
{
    /// <summary>
    /// Interface for generating CIM/XML streams based on aggregation results
    /// </summary>
    public interface ICimXmlResultSerializer
    {
        /// <summary>
        /// Serialize aggregation result to an XML stream using a CIM/XML schema
        /// </summary>
        /// <param name="results">Aggregation results</param>
        /// <param name="stream">Stream to resulting CIM/XML</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task SerializeToStreamAsync(IEnumerable<ResultData> results, Stream stream);
    }
}
