// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
