using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.TestData.Generator
{
    public static class TestDataGenerator
    {
        [FunctionName("BlobTrigger")]
        public static void Run([BlobTrigger("test-data-source/{name}", Connection = "TEST_DATA_SOURCE_CONNECTION_STRING")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
