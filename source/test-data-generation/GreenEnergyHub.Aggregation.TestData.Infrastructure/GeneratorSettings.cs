using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure
{
    public class GeneratorSettings
    {
        public string MasterDataStorageConnectionString { get; set; }

        public string ChargesContainerName { get; set; }

        public string ChargeLinkContainerName { get; set; }

        public string MarketRolesContainerName { get; set; }

        public string MeteringPointContainerName { get; set; }
    }
}
