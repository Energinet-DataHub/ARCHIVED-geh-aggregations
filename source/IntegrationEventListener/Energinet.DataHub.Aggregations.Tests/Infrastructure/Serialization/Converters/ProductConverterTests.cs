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
    public static class ProductConverterTests
    {
        [Theory]
        [InlineAutoMoqData(@"""Tariff""", Product.Tariff)]
        [InlineAutoMoqData(@"""FuelQuantity""", Product.FuelQuantity)]
        [InlineAutoMoqData(@"""PowerActive""", Product.PowerActive)]
        [InlineAutoMoqData(@"""PowerReactive""", Product.PowerReactive)]
        [InlineAutoMoqData(@"""EnergyActive""", Product.EnergyActive)]
        [InlineAutoMoqData(@"""EnergyReactive""", Product.EnergyReactive)]
        public static void Read_ValidStrings_ReturnsCorrectType(
            string json,
            Product expected,
            [NotNull] JsonSerializerOptions options,
            ProductConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Deserialize<Product>(json, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Read_UnknownString_ThrowsException()
        {
            // Arrange
            const string json = @"""Unknown""";
            var options = new JsonSerializerOptions();
            var sut = new ProductConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Deserialize<Product>(json, options));
        }

        [Theory]
        [InlineAutoMoqData(@"""Tariff""", Product.Tariff)]
        [InlineAutoMoqData(@"""FuelQuantity""", Product.FuelQuantity)]
        [InlineAutoMoqData(@"""PowerActive""", Product.PowerActive)]
        [InlineAutoMoqData(@"""PowerReactive""", Product.PowerReactive)]
        [InlineAutoMoqData(@"""EnergyActive""", Product.EnergyActive)]
        [InlineAutoMoqData(@"""EnergyReactive""", Product.EnergyReactive)]
        public static void Write_ValidValue_ReturnsCorrectString(
            string expected,
            Product product,
            [NotNull] JsonSerializerOptions options,
            ProductConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Serialize(product, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Write_UnknownValue_ThrowsException()
        {
            // Arrange
            const Product product = (Product)999;
            var options = new JsonSerializerOptions();
            var sut = new ProductConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Serialize(product, options));
        }
    }
}
