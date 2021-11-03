using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Energinet.DataHub.Aggregations.Domain;

namespace Energinet.DataHub.Aggregations.Infrastructure.Serialization.Converters
{
    public class MeteringMethodConverter : JsonConverter<MeteringMethod>
    {
        public override MeteringMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return value switch
            {
                "Physical" => MeteringMethod.Physical,
                "Virtual" => MeteringMethod.Virtual,
                "Calculated" => MeteringMethod.Calculated,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Could not read JSON value")
            };
        }

        public override void Write(Utf8JsonWriter writer, MeteringMethod value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            switch (value)
            {
                case MeteringMethod.Physical:
                    writer.WriteStringValue("Physical");
                    break;
                case MeteringMethod.Virtual:
                    writer.WriteStringValue("Virtual");
                    break;
                case MeteringMethod.Calculated:
                    writer.WriteStringValue("Calculated");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Could not write JSON value");
            }
        }
    }
}
