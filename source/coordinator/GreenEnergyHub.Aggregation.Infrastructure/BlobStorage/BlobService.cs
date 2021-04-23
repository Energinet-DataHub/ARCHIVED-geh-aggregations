using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using IronSnappy;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Infrastructure.BlobStorage
{
    public class BlobService : IBlobService
    {
        private readonly ILogger<BlobService> _logger;
        private readonly BlobContainerClient _blobContainerClient;

        public BlobService(CoordinatorSettings coordinatorSettings, ILogger<BlobService> logger)
        {
            _logger = logger;
            try
            {
                var blobServiceClient =
                    new BlobServiceClient(
                        $"DefaultEndpointsProtocol=https;AccountName={coordinatorSettings.InputStorageAccountName};AccountKey={coordinatorSettings.InputStorageAccountKey};EndpointSuffix=core.windows.net");
                _blobContainerClient =
                    blobServiceClient.GetBlobContainerClient(coordinatorSettings.InputStorageContainerName);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Could not start BlobService");
                throw;
            }
        }

        public async Task<Stream> GetBlobStreamAsync(string inputPath, CancellationToken cancellationToken)
        {
            try
            {
                var client = _blobContainerClient.GetBlobClient(inputPath);
                var stream = await client.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                // return a decompressed stream
                return Snappy.OpenReader(stream);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Could not GetBlobStreamAsync");
                throw;
            }
        }
    }
}
