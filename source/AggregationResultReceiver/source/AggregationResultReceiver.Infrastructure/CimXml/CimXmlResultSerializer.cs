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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.CimXml;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Helpers;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.CimXml
{
    public class CimXmlResultSerializer : ICimXmlResultSerializer
    {
        private readonly IBlobStore _blobStore;

        public CimXmlResultSerializer(IBlobStore blobStore)
        {
            _blobStore = blobStore;
        }

        public async Task SerializeToStreamAsync(IEnumerable<XDocument> cimXmlResults)
        {
            foreach (var document in cimXmlResults)
            {
                var stream = new MemoryStream();
                await document.SaveAsync(stream, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);
                stream.Position = 0;
                await _blobStore.UploadStreamToBlobContainerAsync("?", "?", "?", stream).ConfigureAwait(false);
            }
        }
    }
}
