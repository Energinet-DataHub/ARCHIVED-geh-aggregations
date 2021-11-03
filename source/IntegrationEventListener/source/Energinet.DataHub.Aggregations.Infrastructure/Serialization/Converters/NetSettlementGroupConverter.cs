using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Energinet.DataHub.Aggregations.Domain;

namespace Energinet.DataHub.Aggregations.Infrastructure.Serialization.Converters
{
    public class NetSettlementGroupConverter : JsonConverter<NetSettlementGroup>
    {
        public override NetSettlementGroup Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return value switch
            {
                "Zero" => NetSettlementGroup.Zero,
                "One" => NetSettlementGroup.One,
                "Two" => NetSettlementGroup.Two,
                "Three" => NetSettlementGroup.Three,
                "Six" => NetSettlementGroup.Six,
                "NinetyNine" => NetSettlementGroup.NinetyNine,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Could not read JSON value")
            };
        }

        public override void Write(Utf8JsonWriter writer, NetSettlementGroup value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            switch (value)
            {
                case NetSettlementGroup.Zero:
                    writer.WriteStringValue("Zero");
                    break;
                case NetSettlementGroup.One:
                    writer.WriteStringValue("One");
                    break;
                case NetSettlementGroup.Two:
                    writer.WriteStringValue("Two");
                    break;
                case NetSettlementGroup.Three:
                    writer.WriteStringValue("Three");
                    break;
                case NetSettlementGroup.Six:
                    writer.WriteStringValue("Six");
                    break;
                case NetSettlementGroup.NinetyNine:
                    writer.WriteStringValue("NinetyNine");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Could not write JSON value");
            }
        }
    }
}
