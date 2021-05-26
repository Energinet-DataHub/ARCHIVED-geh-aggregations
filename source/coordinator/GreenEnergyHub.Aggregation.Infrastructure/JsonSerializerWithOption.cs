﻿// Copyright 2020 Energinet DataHub A/S
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

using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace GreenEnergyHub.Aggregation.Infrastructure
{
    public class JsonSerializerWithOption : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializerWithOption()
        {
            _options = new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public ValueTask<TValue> DeserializeAsync<TValue>(
            Stream utf8Json,
            CancellationToken cancellationToken = default)
        {
            return JsonSerializer.DeserializeAsync<TValue>(utf8Json, _options, cancellationToken);
        }

        public TValue Deserialize<TValue>(string json)
        {
            return JsonSerializer.Deserialize<TValue>(json, _options);
        }

        public string Serialize<TValue>(TValue value)
        {
            return JsonSerializer.Serialize(value, _options);
        }

        public TValue Deserialize<TValue>(string json)
        {
            return JsonSerializer.Deserialize<TValue>(json, _options);
        }
    }
}
