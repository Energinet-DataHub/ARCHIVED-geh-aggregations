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

namespace Energinet.DataHub.Aggregation.Coordinator.CoordinatorFunction.Common
{
    /// <summary>
    /// Contains names of settings used by the function.
    /// </summary>
    public static class EnvironmentSettingNames
    {
        // Function
        public const string AzureWebJobsStorage = "AzureWebJobsStorage";

        // Environment specific settings
        public const string AppInsightsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";

        // Databricks related configuration settings
        public const string ClusterTimeoutMinutes = "CLUSTER_TIMEOUT_MINUTES";
        public const string ConnectionStringDatabricks = "CONNECTION_STRING_DATABRICKS";
        public const string TokenDatabricks = "TOKEN_DATABRICKS";

        // Databricks paths for python files
        public const string AggregationPythonFile = "AGGREGATION_PYTHON_FILE";
        public const string DataPreparationPythonFile = "DATA_PREPARATION_PYTHON_FILE";
        public const string WholesalePythonFile = "WHOLESALE_PYTHON_FILE";

        // JWT Token auth
        public const string B2CTenantId = "B2C_TENANT_ID";
        public const string BackendServiceAppId = "BACKEND_SERVICE_APP_ID";

        // Database connection strings
        public const string CoordinatorDbConnectionString = "DATABASE_CONNECTIONSTRING";
        public const string MasterDataDbConnectionString = "MASTER_DATA_DATABASE_CONNECTION_STRING";

        // Endpoints used by jobs in Databricks
        public const string SnapshotReceiverUrl = "SNAPSHOT_NOTIFY_URL";
        public const string ResultReceiverUrl = "RESULT_URL";

        //Storage account configuration settings
        public const string SharedStorageAccountKey = "SHARED_STORAGE_ACCOUNT_KEY";
        public const string SharedStorageAccountName = "SHARED_STORAGE_ACCOUNT_NAME";
        public const string DataStorageAccountKey = "DATA_STORAGE_ACCOUNT_KEY";
        public const string DataStorageAccountName = "DATA_STORAGE_ACCOUNT_NAME";
        public const string DataStorageContainerName = "DATA_STORAGE_CONTAINER_NAME";
        public const string SharedStorageAggregationsContainerName = "SHARED_STORAGE_AGGREGATIONS_CONTAINER_NAME";
        public const string SharedStorageTimeSeriesContainerName = "SHARED_STORAGE_TIME_SERIES_CONTAINER_NAME";
        public const string TimeSeriesPointsDeltaTableName = "TIME_SERIES_POINTS_DELTA_TABLE_NAME";

        public const string SnapshotsBasePath = "SNAPSHOTS_BASE_PATH";

        public const string GridLossSystemCorrectionPath = "GRID_LOSS_SYSTEM_CORRECTION_PATH";
    }
}
