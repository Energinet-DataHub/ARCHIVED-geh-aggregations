using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Energinet.DataHub.Aggregations.Domain;

namespace Energinet.DataHub.Aggregations.Infrastructure.Serialization.Converters
{
    public class MeteringPointTypeConverter : JsonConverter<MeteringPointType>
    {
        public override MeteringPointType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return value switch
            {
                "Consumption" => MeteringPointType.Consumption,
                "Production" => MeteringPointType.Production,
                "Exchange" => MeteringPointType.Exchange,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Could not read JSON value")
            };
        }

        public override void Write(Utf8JsonWriter writer, MeteringPointType value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            switch (value)
            {
                case MeteringPointType.Consumption:
                    writer.WriteStringValue("Consumption");
                    break;
                case MeteringPointType.Production:
                    writer.WriteStringValue("Production");
                    break;
                case MeteringPointType.Exchange:
                    writer.WriteStringValue("Exchange");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Could not write JSON value");
            }
        }
    }
}
