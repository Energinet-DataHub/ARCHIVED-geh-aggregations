using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Energinet.DataHub.Aggregations.Domain;

namespace Energinet.DataHub.Aggregations.Infrastructure.Serialization.Converters
{
    public class ConnectionStateConverter : JsonConverter<ConnectionState>
    {
        public override ConnectionState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return value switch
            {
                "New" => ConnectionState.New,
                "Connected" => ConnectionState.Connected,
                "Disconnected" => ConnectionState.Disconnected,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Could not read JSON value")
            };
        }

        public override void Write(Utf8JsonWriter writer, ConnectionState value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            switch (value)
            {
                case ConnectionState.New:
                    writer.WriteStringValue("New");
                    break;
                case ConnectionState.Connected:
                    writer.WriteStringValue("Connected");
                    break;
                case ConnectionState.Disconnected:
                    writer.WriteStringValue("Disconnected");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Could not write JSON value");
            }
        }
    }
}
