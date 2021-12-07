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
using System.Threading.Tasks;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Application
{
    /// <summary>
    /// Data storage handling
    /// </summary>
    public interface IFileStore
    {
        /// <summary>
        /// Upload stream to data storage at given file name
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="content">Stream to upload</param>
        Task UploadConvertedMessageAsync(string fileName, Stream content);

        /// <summary>
        /// Download stream from data storage with given file name
        /// </summary>
        /// /// <param name="fileName">Name of file to download</param>
        /// <returns>Stream</returns>
        Task<Stream> DownloadAggregationResultAsync(string fileName);
    }
}
