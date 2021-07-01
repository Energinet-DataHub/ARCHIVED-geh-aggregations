using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.Models
{
    public class ChargeLink : IStoragebleObject
    {
        public string MeteringPointId { get; set; }

        public string ChargeId { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "StartDate")]
        public string FromDate { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "EndDate")]
        public string ToDate { get; set; }
    }
}
