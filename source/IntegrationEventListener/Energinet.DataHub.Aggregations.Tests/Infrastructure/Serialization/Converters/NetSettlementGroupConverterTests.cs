using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Infrastructure.Serialization.Converters;
using Energinet.DataHub.Aggregations.Tests.Attributes;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.Infrastructure.Serialization.Converters
{
    [UnitTest]
    public static class NetSettlementGroupConverterTests
    {
        [Theory]
        [InlineAutoMoqData(@"""Zero""", NetSettlementGroup.Zero)]
        [InlineAutoMoqData(@"""One""", NetSettlementGroup.One)]
        [InlineAutoMoqData(@"""Two""", NetSettlementGroup.Two)]
        [InlineAutoMoqData(@"""Three""", NetSettlementGroup.Three)]
        [InlineAutoMoqData(@"""Six""", NetSettlementGroup.Six)]
        [InlineAutoMoqData(@"""NinetyNine""", NetSettlementGroup.NinetyNine)]
        public static void Read_ValidStrings_ReturnsCorrectType(
            string json,
            NetSettlementGroup expected,
            [NotNull] JsonSerializerOptions options,
            NetSettlementGroupConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Deserialize<NetSettlementGroup>(json, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Read_UnknownString_ThrowsException()
        {
            // Arrange
            const string json = @"""Unknown""";
            var options = new JsonSerializerOptions();
            var sut = new NetSettlementGroupConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Deserialize<NetSettlementGroup>(json, options));
        }

        [Theory]
        [InlineAutoMoqData(@"""Zero""", NetSettlementGroup.Zero)]
        [InlineAutoMoqData(@"""One""", NetSettlementGroup.One)]
        [InlineAutoMoqData(@"""Two""", NetSettlementGroup.Two)]
        [InlineAutoMoqData(@"""Three""", NetSettlementGroup.Three)]
        [InlineAutoMoqData(@"""Six""", NetSettlementGroup.Six)]
        [InlineAutoMoqData(@"""NinetyNine""", NetSettlementGroup.NinetyNine)]
        public static void Write_ValidValue_ReturnsCorrectString(
            string expected,
            NetSettlementGroup netSettlementGroup,
            [NotNull] JsonSerializerOptions options,
            NetSettlementGroupConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Serialize(netSettlementGroup, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Write_UnknownValue_ThrowsException()
        {
            // Arrange
            const NetSettlementGroup netSettlementGroup = (NetSettlementGroup)999;
            var options = new JsonSerializerOptions();
            var sut = new NetSettlementGroupConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Serialize(netSettlementGroup, options));
        }
    }
}
