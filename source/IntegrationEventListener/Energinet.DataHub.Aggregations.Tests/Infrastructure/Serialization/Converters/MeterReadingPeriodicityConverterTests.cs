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
    public static class MeterReadingPeriodicityConverterTests
    {
        [Theory]
        [InlineAutoMoqData(@"""Hourly""", MeterReadingPeriodicity.Hourly)]
        [InlineAutoMoqData(@"""Quarterly""", MeterReadingPeriodicity.Quarterly)]
        public static void Read_ValidStrings_ReturnsCorrectPeriodicity(
            string json,
            MeterReadingPeriodicity expected,
            [NotNull] JsonSerializerOptions options,
            MeterReadingPeriodicityConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Deserialize<MeterReadingPeriodicity>(json, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Read_UnknownString_ThrowsException()
        {
            // Arrange
            const string json = @"""Unknown""";
            var options = new JsonSerializerOptions();
            var sut = new MeterReadingPeriodicityConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Deserialize<MeterReadingPeriodicity>(json, options));
        }

        [Theory]
        [InlineAutoMoqData(@"""Hourly""", MeterReadingPeriodicity.Hourly)]
        [InlineAutoMoqData(@"""Quarterly""", MeterReadingPeriodicity.Quarterly)]
        public static void Write_ValidValue_ReturnsCorrectString(
            string expected,
            MeterReadingPeriodicity meterReadingPeriodicity,
            [NotNull] JsonSerializerOptions options,
            MeterReadingPeriodicityConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Serialize(meterReadingPeriodicity, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Write_UnknownValue_ThrowsException()
        {
            // Arrange
            const MeterReadingPeriodicity meterReadingPeriodicity = (MeterReadingPeriodicity)999;
            var options = new JsonSerializerOptions();
            var sut = new MeterReadingPeriodicityConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Serialize(meterReadingPeriodicity, options));
        }
    }
}
