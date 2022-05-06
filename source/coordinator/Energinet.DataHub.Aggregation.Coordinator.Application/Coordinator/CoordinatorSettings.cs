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

        public string ConnectionStringDatabricks { get; set;  } = null!;

        public string TokenDatabricks { get; set; } = null!;

        public Uri ResultUrl { get; set; } = null!;

        public Uri SnapshotNotifyUrl { get; set; } = null!;

        public string DataStorageAccountName { get; set; } = null!;

        public string DataStorageAccountKey { get; set; } = null!;

        public string DataStorageContainerName { get; set; } = null!;

        public string SharedStorageAccountName { get; set; } = null!;

        public string SharedStorageAccountKey { get; set; } = null!;

        public string SharedStorageAggregationsContainerName { get; set; } = null!;

        public string SharedStorageTimeSeriesContainerName { get; set; } = null!;

        public string TimeSeriesPointsDeltaTableName { get; set; } = null!;

        public string SnapshotsBasePath { get; set; } = null!;

        public string AggregationPythonFile { get; set; } = null!;

        public string WholesalePythonFile { get; set; } = null!;

        public string DataPreparationPythonFile { get; set; } = null!;

        public int ClusterTimeoutMinutes { get; set; }

        public string B2CTenantId { get; set; } = null!;

        public string BackendServiceAppId { get; set; } = null!;

        public string MasterDataDatabaseConnectionString { get; set; } = null!;
    }
}
