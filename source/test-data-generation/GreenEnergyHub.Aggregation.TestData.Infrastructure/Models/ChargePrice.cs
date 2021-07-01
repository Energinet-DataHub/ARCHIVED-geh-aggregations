using System;
using System.Collections.Generic;
using System.Text;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;

namespace GreenEnergyHub.Aggregation.TestData.Application.Parsers
{
    public class ChargePrices : IStoragebleObject
    {
        public string ChargeId { get; set; }

        public string ChargePrice { get; set; }

        public string Time { get; set; }
    }
}
