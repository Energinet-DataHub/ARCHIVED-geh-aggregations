using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using AggregationResultReceiver.Domain;
using AggregationResultReceiver.Infrastructure.CimXml;
using Energinet.DataHub.AggregationResultReceiver.Tests.Attributes;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.AggregationResultReceiver.Tests
{
    [UnitTest]
    public class CimXmlResultSerializerTests
    {
        private const int NoOfResultsInBundle = 10;

        [Theory]
        [InlineAutoMoqData("Assets/ExpectedAggregationResultForFlexConsumptionPerGridAreaMdr.blob")]
        public async Task SerializeToStreamAsync_ValidInput_ReturnsCorrectsXml(
            string expected,
            [NotNull] CimXmlResultSerializer sut)
        {
            var input = GetResultData();
            await using var actualStream = new MemoryStream();
            await sut.SerializeToStreamAsync(input, actualStream);
            var actual = actualStream.ToString();
            Assert.Equal(expected, actual);
        }

        private static IEnumerable<ResultData> GetResultData()
        {
            // Læs JSON
            // Deserialiser json -> List<ResultData>
            var resultData = new List<ResultData>();

            for (var i = 0; i < NoOfResultsInBundle; i++)
            {
                resultData.Add(new ResultData(
                    "JobId",
                    "SnapshotId",
                    "ResultId",
                    "ResultName",
                    "GridArea",
                    "InGridArea",
                    "OutGridArea",
                    "BalanceResponsibleId",
                    "EnergySupplierId",
                    "StartDateTime",
                    "EndDateTime",
                    "Resolution",
                    12.34M,
                    "MeteringPointType",
                    "SettlementMethod"));
            }

            return resultData;
        }
    }
}
