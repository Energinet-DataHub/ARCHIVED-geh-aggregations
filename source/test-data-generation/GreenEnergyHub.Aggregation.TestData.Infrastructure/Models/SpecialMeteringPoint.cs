using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.Models
{
    public class SpecialMeteringPoint : IStoragebleObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id => Guid.NewGuid().ToString();

        [JsonProperty(PropertyName = "metering_point_id")]
        public string MeteringPointID { get; set; }

        [JsonProperty(PropertyName = "energy_supplier")]
        public string EnergySupplier { get; set; }

        [JsonProperty(PropertyName = "grid_area")]
        public string GridArea { get; set; }

        [JsonProperty(PropertyName = "is_grid_loss")]
        public string IsGridLoss { get; set; }

        [JsonProperty(PropertyName = "is_system_correction")]
        public string IsSystemCorrection { get; set; }

        [JsonProperty(PropertyName = "from_date")]
        public string FromDate { get; set; }

        [JsonProperty(PropertyName = "to_date")]
        public string ToDate { get; set; }
    }
}
