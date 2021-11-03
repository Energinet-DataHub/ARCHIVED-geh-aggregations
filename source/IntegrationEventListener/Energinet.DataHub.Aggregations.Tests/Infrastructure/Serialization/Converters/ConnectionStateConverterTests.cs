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
    public static class ConnectionStateConverterTests
    {
        [Theory]
        [InlineAutoMoqData(@"""New""", ConnectionState.New)]
        [InlineAutoMoqData(@"""Connected""", ConnectionState.Connected)]
        [InlineAutoMoqData(@"""Disconnected""", ConnectionState.Disconnected)]
        public static void Read_ValidStrings_ReturnsCorrectState(
            string json,
            ConnectionState connectionState,
            [NotNull] JsonSerializerOptions options,
            ConnectionStateConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Deserialize<ConnectionState>(json, options);

            // Assert
            Assert.Equal(connectionState, actual);
        }

        [Fact]
        public static void Read_UnknownString_ThrowsException()
        {
            // Arrange
            const string json = @"""Unknown""";
            var options = new JsonSerializerOptions();
            var sut = new ConnectionStateConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Deserialize<ConnectionState>(json, options));
        }

        [Theory]
        [InlineAutoMoqData(@"""New""", ConnectionState.New)]
        [InlineAutoMoqData(@"""Connected""", ConnectionState.Connected)]
        [InlineAutoMoqData(@"""Disconnected""", ConnectionState.Disconnected)]
        public static void Write_ValidValue_ReturnsCorrectString(
            string json,
            ConnectionState connectionState,
            [NotNull] JsonSerializerOptions options,
            ConnectionStateConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Serialize(connectionState, options);

            // Assert
            Assert.Equal(json, actual);
        }

        [Fact]
        public static void Write_UnknownState_ThrowsException()
        {
            // Arrange
            const ConnectionState connectionState = (ConnectionState)999;
            var options = new JsonSerializerOptions();
            var sut = new ConnectionStateConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Serialize(connectionState, options));
        }
    }
}
