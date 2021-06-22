using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GreenEnergyHub.Aggregation.Infrastructure.CosmosDb
{
   public class StreamInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }
    }
}
