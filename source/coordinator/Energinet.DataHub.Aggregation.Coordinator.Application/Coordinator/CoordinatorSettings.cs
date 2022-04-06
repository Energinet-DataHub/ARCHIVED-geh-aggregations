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

namespace Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator
{
    public class CoordinatorSettings
    {
        public const string ClusterName = "Aggregations Autoscaling";

        public string ConnectionStringDatabricks { get; set;  }

        public string TokenDatabricks { get; set; }

        public Uri ResultUrl { get; set; }

        public Uri SnapshotNotifyUrl { get; set; }

        public string DataStorageAccountName { get; set; }

        public string DataStorageAccountKey { get; set; }

        public string DataStorageContainerName { get; set; }

        public string SharedStorageAccountName { get; set; }

        public string SharedStorageAccountKey { get; set; }

        public string SharedStorageAggregationsContainerName { get; set; }

        public string SharedStorageTimeSeriesContainerName { get; set; }

        public string TimeSeriesPointsDeltaTableName { get; set; }

        public string GridLossSystemCorrectionPath { get; set; }

        public string SnapshotsBasePath { get; set; }

        public string AggregationPythonFile { get; set; }

        public string WholesalePythonFile { get; set; }

        public string DataPreparationPythonFile { get; set; }

        public int ClusterTimeoutMinutes { get; set; }

        public string B2CTenantId { get; set; }

        public string BackendServiceAppId { get; set; }

        public string MasterDataDatabaseConnectionString { get; set; }
    }
}
