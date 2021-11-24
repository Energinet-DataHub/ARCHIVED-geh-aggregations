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
using Azure;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Storage
{
    /// <summary>
    /// Generic interface for blob interaction
    /// </summary>
    public interface IBlobStorage
    {
        /// <summary>
        /// Creates a new blob
        /// </summary>
        Task UploadBlobAsync(string blobName, Stream content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Download the content of a blob
        /// </summary>
        /// <returns>A <see cref="Response{T}"/> with the blob content</returns>
        Task<Stream> DownloadBlobAsync(string blobName, CancellationToken cancellationToken = default);
    }
}
