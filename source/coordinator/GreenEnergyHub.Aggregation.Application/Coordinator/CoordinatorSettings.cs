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

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    public class CoordinatorSettings
    {
        public const string ClusterName = "Aggregation Autoscaling";

        public const string ClusterAggregationJobName = "Aggregation JobMetadata";

        public const string ClusterWholesaleJobName = "Wholesale JobMetadata";

        public const string AdjustedFlexConsumptionName = "AdjustedFlexConsumption";

        public const string HourlyProductionName = "HourlyProduction";

        public const string HourlyConsumptionName = "HourlyConsumption";

        public const string AdjustedHourlyProductionName = "AdjustedHourlyProduction";

        public const string FlexConsumptionName = "FlexConsumption";

        public const string ExchangeName = "Exchange";

        public const string ExchangeNeighbourName = "ExchangeNeighbour";

        public string ConnectionStringDatabricks { get; set;  }

        public string HostKey { get; set; }

        public string TokenDatabricks { get; set; }

        public Uri ResultUrl { get; set; }

        public Uri SnapshotUrl { get; set; }

        public string DataStorageAccountName { get; set; }

        public string DataStorageAccountKey { get; set; }

        public string DataStorageContainerName { get; set; }

        public string TimeSeriesPath { get; set; }

        public string PersistLocation { get; set; }

        public string TelemetryInstrumentationKey { get; set; }

        public string AggregationPythonFile { get; set; }

        public string WholesalePythonFile { get; set; }

        public int ClusterTimeoutMinutes { get; set; }

        public string CosmosAccountEndpoint { get; set; }

        public string CosmosAccountKey { get; set; }

        public string CosmosDatabase { get; set; }

        public string CosmosContainerMeteringPoints { get; set; }

        public string CosmosContainerMarketRoles { get; set; }

        public string CosmosContainerCharges { get; set; }

        public string CosmosContainerChargeLinks { get; set; }

        public string CosmosContainerChargePrices { get; set; }

        public string CosmosContainerGridLossSysCorr { get; set; }

        public string CosmosContainerEsBrpRelations { get; set; }
    }
}
