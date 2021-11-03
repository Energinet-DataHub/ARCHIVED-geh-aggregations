using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Energinet.DataHub.Aggregations.Domain;

namespace Energinet.DataHub.Aggregations.Infrastructure.Serialization.Converters
{
    public class SettlementMethodConverter : JsonConverter<SettlementMethod>
    {
        public override SettlementMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return value switch
            {
                "Flex" => SettlementMethod.Flex,
                "Profiled" => SettlementMethod.Profiled,
                "NonProfiled" => SettlementMethod.NonProfiled,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Could not read JSON value")
            };
        }

        public override void Write(Utf8JsonWriter writer, SettlementMethod value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            switch (value)
            {
                case SettlementMethod.Flex:
                    writer.WriteStringValue("Flex");
                    break;
                case SettlementMethod.Profiled:
                    writer.WriteStringValue("Profiled");
                    break;
                case SettlementMethod.NonProfiled:
                    writer.WriteStringValue("NonProfiled");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Could not write JSON value");
            }
        }
    }
}
