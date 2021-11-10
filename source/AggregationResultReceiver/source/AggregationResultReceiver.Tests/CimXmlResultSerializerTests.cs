using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;
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

        private string EmbeddedResourceAssetReader(string fileName)
        {
            var resource = $"Energinet.DataHub.AggregationResultReceiver.Tests.Assets.{fileName}";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            if (stream == null) return string.Empty;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private IEnumerable<ResultData> GetResultData()
        {
            var jsonArrayOfString = EmbeddedResourceAssetReader("result_mock_flex_consumption_per_grid_area.json");
            var resultDataArray = JsonSerializer.Deserialize<List<ResultData>>(jsonArrayOfString);
            return resultDataArray;
        }
    }
}
