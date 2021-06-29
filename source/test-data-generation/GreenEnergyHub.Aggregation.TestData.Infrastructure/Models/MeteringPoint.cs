using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.Models
{
    public class MeteringPoint
    {
        public MeteringPoint(string s)
        {
            Id = s;
        }

        public string Id { get; set; }
    }
}
