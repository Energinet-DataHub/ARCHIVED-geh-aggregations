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

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    public class CoordinatorSettings
    {
        public const string ClusterName = "Aggregation Autoscaling";

        public const string ClusterJobName = "Aggregation Job";

        public const string AdjustedFlexConsumptionName = "AdjustedFlexConsumption";

        public const string HourlyProductionName = "HourlyProduction";

        public const string HourlyConsumptionName = "HourlyConsumption";

        public const string AdjustedHourlyProductionName = "AdjustedHourlyProduction";

        public const string FlexConsumptionName = "FlexConsumption";

        public CoordinatorSettings(
            string connectionStringDatabricks,
            string tokenDatabricks,
            string inputStorageAccountName,
            string inputStorageAccountKey,
            string inputStorageContainerName,
            string telemetryInstrumentationKey,
            string resultUrl,
            string pythonFile,
            string clusterTimeOutMinutes)
        {
            ConnectionStringDatabricks = connectionStringDatabricks;
            TokenDatabricks = tokenDatabricks;
            ResultUrl = resultUrl;
            InputStorageAccountName = inputStorageAccountName;
            InputStorageAccountKey = inputStorageAccountKey;
            InputStorageContainerName = inputStorageContainerName;
            TelemetryInstrumentationKey = telemetryInstrumentationKey;
            PythonFile = pythonFile;
            ClusterTimeoutMinutes = int.Parse(clusterTimeOutMinutes);
        }

        public string ConnectionStringDatabricks { get; }

        public string TokenDatabricks { get; }

        public string ResultUrl { get; }

        public string InputStorageAccountName { get; set; }

        public string InputStorageAccountKey { get; set; }

        public string InputStorageContainerName { get; set; }

        public string TelemetryInstrumentationKey { get; set; }

        public string PythonFile { get; set; }

        public int ClusterTimeoutMinutes { get; set; }
    }
}
