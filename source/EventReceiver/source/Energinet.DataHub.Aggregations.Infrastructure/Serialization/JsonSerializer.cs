using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.Interfaces;
using NodaTime.Serialization.SystemTextJson;

namespace Energinet.DataHub.Aggregations.Infrastructure.Serialization
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializer()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(NodaConverters.InstantConverter);
        }

        public ValueTask<object> DeserializeAsync(Stream utf8Json, Type returnType)
        {
            return System.Text.Json.JsonSerializer.DeserializeAsync(utf8Json, returnType, _options) !;
        }

        public TValue Deserialize<TValue>(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<TValue>(json, _options) !;
        }

        public object Deserialize(string json, Type returnType)
        {
            return System.Text.Json.JsonSerializer.Deserialize(json, returnType, _options) !;
        }

        public string Serialize<TValue>(TValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return System.Text.Json.JsonSerializer.Serialize<object>(value, _options);
        }
    }
}
