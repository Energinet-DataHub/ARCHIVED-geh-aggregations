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
    public static class QuantityUnitConverterTests
    {
        [Theory]
        [InlineAutoMoqData(@"""Wh""", QuantityUnit.Wh)]
        [InlineAutoMoqData(@"""Kwh""", QuantityUnit.Kwh)]
        [InlineAutoMoqData(@"""Mwh""", QuantityUnit.Mwh)]
        [InlineAutoMoqData(@"""Gwh""", QuantityUnit.Gwh)]
        public static void Read_ValidStrings_ReturnsCorrectType(
            string json,
            QuantityUnit expected,
            [NotNull] JsonSerializerOptions options,
            QuantityUnitConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Deserialize<QuantityUnit>(json, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Read_UnknownString_ThrowsException()
        {
            // Arrange
            const string json = @"""Unknown""";
            var options = new JsonSerializerOptions();
            var sut = new QuantityUnitConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Deserialize<QuantityUnit>(json, options));
        }

        [Theory]
        [InlineAutoMoqData(@"""Wh""", QuantityUnit.Wh)]
        [InlineAutoMoqData(@"""Kwh""", QuantityUnit.Kwh)]
        [InlineAutoMoqData(@"""Mwh""", QuantityUnit.Mwh)]
        [InlineAutoMoqData(@"""Gwh""", QuantityUnit.Gwh)]
        public static void Write_ValidValue_ReturnsCorrectString(
            string expected,
            QuantityUnit quantityUnit,
            [NotNull] JsonSerializerOptions options,
            QuantityUnitConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Serialize(quantityUnit, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Write_UnknownValue_ThrowsException()
        {
            // Arrange
            const QuantityUnit quantityUnit = (QuantityUnit)999;
            var options = new JsonSerializerOptions();
            var sut = new QuantityUnitConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Serialize(quantityUnit, options));
        }
    }
}
