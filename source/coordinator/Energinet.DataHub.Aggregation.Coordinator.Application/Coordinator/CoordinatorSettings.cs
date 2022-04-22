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

        public string ConnectionStringDatabricks { get; set; } = default!;

        public string TokenDatabricks { get; set; } = default!;

        public Uri ResultUrl { get; set; } = default!;

        public Uri SnapshotNotifyUrl { get; set; } = default!;

        public string DataStorageAccountName { get; set; } = default!;

        public string DataStorageAccountKey { get; set; } = default!;

        public string DataStorageContainerName { get; set; } = default!;

        public string SharedStorageAccountName { get; set; } = default!;

        public string SharedStorageAccountKey { get; set; } = default!;

        public string SharedStorageAggregationsContainerName { get; set; } = default!;

        public string SharedStorageTimeSeriesContainerName { get; set; } = default!;

        public string TimeSeriesPointsDeltaTableName { get; set; } = default!;

        public string GridLossSystemCorrectionPath { get; set; } = default!;

        public string SnapshotsBasePath { get; set; } = default!;

        public string AggregationPythonFile { get; set; } = default!;

        public string WholesalePythonFile { get; set; } = default!;

        public string DataPreparationPythonFile { get; set; } = default!;

        public int ClusterTimeoutMinutes { get; set; } = default!;

        public string B2CTenantId { get; set; } = default!;

        public string BackendServiceAppId { get; set; } = default!;

        public string MasterDataDatabaseConnectionString { get; set; } = default!;
    }
}
