using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Energinet.DataHub.Aggregations.Domain;

namespace Energinet.DataHub.Aggregations.Infrastructure.Serialization.Converters
{
    public class QuantityUnitConverter : JsonConverter<QuantityUnit>
    {
        public override QuantityUnit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return value switch
            {
                "Wh" => QuantityUnit.Wh,
                "Kwh" => QuantityUnit.Kwh,
                "Mwh" => QuantityUnit.Mwh,
                "Gwh" => QuantityUnit.Gwh,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Could not read JSON value")
            };
        }

        public override void Write(Utf8JsonWriter writer, QuantityUnit value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            switch (value)
            {
                case QuantityUnit.Wh:
                    writer.WriteStringValue("Wh");
                    break;
                case QuantityUnit.Kwh:
                    writer.WriteStringValue("Kwh");
                    break;
                case QuantityUnit.Mwh:
                    writer.WriteStringValue("Mwh");
                    break;
                case QuantityUnit.Gwh:
                    writer.WriteStringValue("Gwh");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Could not write JSON value");
            }
        }
    }
}
