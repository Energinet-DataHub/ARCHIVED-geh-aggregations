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

        public string Serialize<TValue>(TValue value)
        {
            return JsonSerializer.Serialize(value, _options);
        }
    }
}
