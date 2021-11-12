using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AggregationResultReceiver.Domain;
using AggregationResultReceiver.Infrastructure.CimXml;
using Energinet.DataHub.AggregationResultReceiver.Tests.Attributes;
using Newtonsoft.Json;
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
            var input = GetResultData(sut);
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

        private List<ResultData> JsonMultipleContentReader(string jsonContent)
        {
            var resultDataArray = new List<ResultData>();
            JsonTextReader reader = new JsonTextReader(new StringReader(jsonContent));
            reader.SupportMultipleContent = true;
            JsonSerializer serializer = new JsonSerializer();
            while (true)
            {
                if (!reader.Read())
                {
                    break;
                }

                ResultData resultData = serializer.Deserialize<ResultData>(reader);

                resultDataArray.Add(resultData);
            }

            return resultDataArray;
        }

        private IEnumerable<ResultData> GetResultData(CimXmlResultSerializer sut)
        {
            var list = new List<string>()
            {
                "result_mock_flex_consumption_per_grid_area.json",
                "result_mock_hourly_consumption_per_grid_area.json",
                "result_mock_net_exchange_per_grid_area.json",
                "result_mock_production_per_grid_area.json",
                "result_mock_total_consumption.json",
            };
            var resultDataArray = new List<ResultData>();

            foreach (var file in list)
            {
                resultDataArray.AddRange(JsonMultipleContentReader(EmbeddedResourceAssetReader(file)));
            }

            var test = sut.MapToCimXml(resultDataArray);
            // test
            // var grp = resultDataArray!
            //     .GroupBy(x => x.GridArea)
            //     .Select(g => g
            //         .GroupBy(y => y.ResultName)
            //         .Select(h => h
            //             .ToList())
            //         .ToList())
            //     .ToList();
            return resultDataArray;
        }
    }
}
