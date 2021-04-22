using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GreenEnergyHub.Aggregation.Infrastructure.BlobStorage
{
    /// <summary>
    /// A service for reading into azure blobstorage
    /// </summary>
    public interface IBlobService
    {
        /// <summary>
        /// Returns a decompressed stream with the data in the provided path
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Stream</returns>
        Task<Stream> GetBlobStreamAsync(string inputPath, CancellationToken cancellationToken);
    }
}
