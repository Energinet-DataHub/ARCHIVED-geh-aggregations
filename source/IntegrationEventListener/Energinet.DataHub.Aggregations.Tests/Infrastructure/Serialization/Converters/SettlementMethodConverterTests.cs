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
    public static class SettlementMethodConverterTests
    {
        [Theory]
        [InlineAutoMoqData(@"""Flex""", SettlementMethod.Flex)]
        [InlineAutoMoqData(@"""Profiled""", SettlementMethod.Profiled)]
        [InlineAutoMoqData(@"""NonProfiled""", SettlementMethod.NonProfiled)]
        public static void Read_ValidStrings_ReturnsCorrectType(
            string json,
            SettlementMethod expected,
            [NotNull] JsonSerializerOptions options,
            SettlementMethodConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Deserialize<SettlementMethod>(json, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Read_UnknownString_ThrowsException()
        {
            // Arrange
            const string json = @"""Unknown""";
            var options = new JsonSerializerOptions();
            var sut = new SettlementMethodConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Deserialize<SettlementMethod>(json, options));
        }

        [Theory]
        [InlineAutoMoqData(@"""Flex""", SettlementMethod.Flex)]
        [InlineAutoMoqData(@"""Profiled""", SettlementMethod.Profiled)]
        [InlineAutoMoqData(@"""NonProfiled""", SettlementMethod.NonProfiled)]
        public static void Write_ValidValue_ReturnsCorrectString(
            string expected,
            SettlementMethod settlementMethod,
            [NotNull] JsonSerializerOptions options,
            SettlementMethodConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Serialize(settlementMethod, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Write_UnknownValue_ThrowsException()
        {
            // Arrange
            const SettlementMethod settlementMethod = (SettlementMethod)999;
            var options = new JsonSerializerOptions();
            var sut = new SettlementMethodConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Serialize(settlementMethod, options));
        }
    }
}
