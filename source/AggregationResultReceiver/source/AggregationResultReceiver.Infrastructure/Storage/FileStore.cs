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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Configurations;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Storage
{
    public class FileStore : IFileStore
    {
        private readonly IBlobStorage _aggregationResultsBlobStorage;
        private readonly IBlobStorage _convertedMessagesBlobStorage;

        public FileStore(FileStoreConfiguration fileStoreSettings)
        {
            if (fileStoreSettings == null) throw new ArgumentNullException(nameof(fileStoreSettings));
            _aggregationResultsBlobStorage = new BlobStorage(fileStoreSettings);
            _convertedMessagesBlobStorage = new BlobStorage(fileStoreSettings);
        }

        public async Task UploadConvertedMessageAsync(string fileName, Stream content)
        {
            await _aggregationResultsBlobStorage.UploadBlobAsync(fileName, content, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<Stream> DownloadAggregationResultAsync(string fileName)
        {
            return await _convertedMessagesBlobStorage.DownloadBlobAsync(fileName).ConfigureAwait(false);
        }
    }
}
