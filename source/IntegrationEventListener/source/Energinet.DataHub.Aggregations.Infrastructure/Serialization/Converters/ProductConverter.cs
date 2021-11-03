using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Energinet.DataHub.Aggregations.Domain;

namespace Energinet.DataHub.Aggregations.Infrastructure.Serialization.Converters
{
    public class ProductConverter : JsonConverter<Product>
    {
        public override Product Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return value switch
            {
                "Tariff" => Product.Tariff,
                "EnergyActive" => Product.EnergyActive,
                "EnergyReactive" => Product.EnergyReactive,
                "FuelQuantity" => Product.FuelQuantity,
                "PowerActive" => Product.PowerActive,
                "PowerReactive" => Product.PowerReactive,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Could not read JSON value")
            };
        }

        public override void Write(Utf8JsonWriter writer, Product value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            switch (value)
            {
                case Product.Tariff:
                    writer.WriteStringValue("Tariff");
                    break;
                case Product.EnergyActive:
                    writer.WriteStringValue("EnergyActive");
                    break;
                case Product.EnergyReactive:
                    writer.WriteStringValue("EnergyReactive");
                    break;
                case Product.FuelQuantity:
                    writer.WriteStringValue("FuelQuantity");
                    break;
                case Product.PowerActive:
                    writer.WriteStringValue("PowerActive");
                    break;
                case Product.PowerReactive:
                    writer.WriteStringValue("PowerReactive");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Could not write JSON value");
            }
        }
    }
}
