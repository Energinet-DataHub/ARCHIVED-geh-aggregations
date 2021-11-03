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
    public static class MeteringMethodConverterTests
    {
        [Theory]
        [InlineAutoMoqData(@"""Physical""", MeteringMethod.Physical)]
        [InlineAutoMoqData(@"""Virtual""", MeteringMethod.Virtual)]
        [InlineAutoMoqData(@"""Calculated""", MeteringMethod.Calculated)]
        public static void Read_ValidStrings_ReturnsCorrectMethod(
            string json,
            MeteringMethod expected,
            [NotNull] JsonSerializerOptions options,
            MeteringMethodConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Deserialize<MeteringMethod>(json, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Read_UnknownString_ThrowsException()
        {
            // Arrange
            const string json = @"""Unknown""";
            var options = new JsonSerializerOptions();
            var sut = new MeteringMethodConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Deserialize<MeteringMethod>(json, options));
        }

        [Theory]
        [InlineAutoMoqData(@"""Physical""", MeteringMethod.Physical)]
        [InlineAutoMoqData(@"""Virtual""", MeteringMethod.Virtual)]
        [InlineAutoMoqData(@"""Calculated""", MeteringMethod.Calculated)]
        public static void Write_ValidValue_ReturnsCorrectString(
            string expected,
            MeteringMethod meteringMethod,
            [NotNull] JsonSerializerOptions options,
            MeteringMethodConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Serialize(meteringMethod, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Write_UnknownValue_ThrowsException()
        {
            // Arrange
            const MeteringMethod meteringMethod = (MeteringMethod)999;
            var options = new JsonSerializerOptions();
            var sut = new MeteringMethodConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Serialize(meteringMethod, options));
        }
    }
}
