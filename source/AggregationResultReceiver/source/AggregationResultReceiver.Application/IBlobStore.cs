using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Application
{
    /// <summary>
    /// BlobStore
    /// </summary>
    public interface IBlobStore
    {
        /// <summary>
        /// GetResultDataFromBlob
        /// </summary>
        Task<string> DownloadFromBlobContainerAsync(string blobName, string containerName, string connectionString);

        /// <summary>
        /// GetResultDataFromBlob
        /// </summary>
        Task UploadToBlobContainerAsync(string blobName, string containerName, string connectionString, Stream stream);
    }
}
