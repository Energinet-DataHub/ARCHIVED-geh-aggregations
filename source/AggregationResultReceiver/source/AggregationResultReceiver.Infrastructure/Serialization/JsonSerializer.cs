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
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Serialization;
using NodaTime.Serialization.SystemTextJson;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Serialization
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializer()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(NodaConverters.InstantConverter);
            _options.PropertyNamingPolicy = new JsonSerializerNamingPolicy();
        }

        public TValue? Deserialize<TValue>(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            return System.Text.Json.JsonSerializer.Deserialize<TValue>(json, _options);
        }

        public IEnumerable<TValue> DeserializeMultipleContent<TValue>(Stream content)
        {
            using var reader = new StreamReader(content);
            var json = reader.ReadToEnd();
            return DeserializeMultipleContent<TValue>(json);
        }

        private IEnumerable<TValue> DeserializeMultipleContent<TValue>(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            string pattern = @"(})";
            var values = Regex.Split(json, pattern);
            for (var i = 0; i < values.Length - 2; i += 2)
            {
                var combined = values[i] + values[i + 1];
                var deserialized = System.Text.Json.JsonSerializer.Deserialize<TValue>(combined, _options);
                if (deserialized == null) throw new Exception($"Could not deserialize string: {combined}");
                yield return deserialized;
            }
        }
    }
}
