using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Energinet.DataHub.Aggregations.Domain;

namespace Energinet.DataHub.Aggregations.Infrastructure.Serialization.Converters
{
    public class MeterReadingPeriodicityConverter : JsonConverter<MeterReadingPeriodicity>
    {
        public override MeterReadingPeriodicity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return value switch
            {
                "Hourly" => MeterReadingPeriodicity.Hourly,
                "Quarterly" => MeterReadingPeriodicity.Quarterly,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Could not read JSON value")
            };
        }

        public override void Write(Utf8JsonWriter writer, MeterReadingPeriodicity value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            switch (value)
            {
                case MeterReadingPeriodicity.Hourly:
                    writer.WriteStringValue("Hourly");
                    break;
                case MeterReadingPeriodicity.Quarterly:
                    writer.WriteStringValue("Quarterly");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Could not write JSON value");
            }
        }
    }
}
