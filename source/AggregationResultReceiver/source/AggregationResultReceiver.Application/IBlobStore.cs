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
        /// DownloadFromBlobContainerAsync
        /// </summary>
        Task<string> DownloadFromBlobContainerAsync(string connectionString, string containerName, string blobName);

        /// <summary>
        /// UploadToBlobContainerAsync
        /// </summary>
        Task<string> UploadStreamToBlobContainerAsync(string connectionString, string containerName, string blobName, Stream stream);

        /// <summary>
        /// DeleteFromBlobContainerAsync
        /// </summary>
        Task<string> DeleteFromBlobContainerAsync(string connectionString, string containerName, string blobName);
    }
}
