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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Aggregation.IntegrationTests.Fixtures
{
    public class CoordinatorFunctionAppFixture : FunctionAppFixture
    {
        public CoordinatorFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            AuthorizationConfiguration = new AuthorizationConfiguration();
        }

        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private AzuriteManager AzuriteManager { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            if (hostSettings == null)
            {
                return;
            }

            var buildConfiguration = GetBuildConfiguration();

            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\GreenEnergyHub.Aggregation.Coordinator\\bin\\{buildConfiguration}\\net5.0";

            // The log message we expect in the host log when the host is started and ready to server.
            hostSettings.HostStartedEvent = "Worker process started and initialized";
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            Environment.SetEnvironmentVariable("B2C_TENANT_ID", AuthorizationConfiguration.B2cTenantId);
            Environment.SetEnvironmentVariable("BACKEND_SERVICE_APP_ID", AuthorizationConfiguration.BackendAppId);
            Environment.SetEnvironmentVariable("CONNECTION_STRING_DATABRICKS", AuthorizationConfiguration.BackendAppId);
            Environment.SetEnvironmentVariable("TOKEN_DATABRICKS", AuthorizationConfiguration.BackendAppId);
            Environment.SetEnvironmentVariable("DATA_STORAGE_CONTAINER_NAME", "fake");
            Environment.SetEnvironmentVariable("DATA_STORAGE_ACCOUNT_NAME", "fake");
            Environment.SetEnvironmentVariable("DATA_STORAGE_ACCOUNT_KEY", "fake");
            Environment.SetEnvironmentVariable("SHARED_STORAGE_CONTAINER_NAME", "fake");
            Environment.SetEnvironmentVariable("SHARED_STORAGE_ACCOUNT_NAME", "fake");
            Environment.SetEnvironmentVariable("SHARED_STORAGE_ACCOUNT_KEY", "fake");
            Environment.SetEnvironmentVariable("TIME_SERIES_PATH", "fake");
            Environment.SetEnvironmentVariable("METERING_POINTS_PATH", "fake");
            Environment.SetEnvironmentVariable("MARKET_ROLES_PATH", "fake");
            Environment.SetEnvironmentVariable("CHARGES_PATH", "fake");
            Environment.SetEnvironmentVariable("CHARGE_LINKS_PATH", "fake");
            Environment.SetEnvironmentVariable("CHARGE_PRICES_PATH", "fake");
            Environment.SetEnvironmentVariable("CLUSTER_TIMEOUT_MINUTES", "1");
            Environment.SetEnvironmentVariable("DATABASE_CONNECTIONSTRING", "fake");
            Environment.SetEnvironmentVariable("ES_BRP_RELATIONS_PATH", "fake");
            Environment.SetEnvironmentVariable("GRID_LOSS_SYSTEM_CORRECTION_PATH", "fake");
            Environment.SetEnvironmentVariable("SNAPSHOT_PATH", "https://www.kelz0r.dk/magic/index.php");
            Environment.SetEnvironmentVariable("RESULT_URL", "https://www.kelz0r.dk/magic/index.php");
            Environment.SetEnvironmentVariable("SNAPSHOT_URL", "https://www.kelz0r.dk/magic/index.php");
            Environment.SetEnvironmentVariable("AGGREGATION_PYTHON_FILE", "fake");
            Environment.SetEnvironmentVariable("WHOLESALE_PYTHON_FILE", "fake");
            Environment.SetEnvironmentVariable("DATA_PREPARATION_PYTHON_FILE", "fake");
            /*
             *
            var dataStorageContainerName = StartupConfig.GetConfigurationVariable(config, "DATA_STORAGE_CONTAINER_NAME");
            var dataStorageAccountName = StartupConfig.GetConfigurationVariable(config, "DATA_STORAGE_ACCOUNT_NAME");
            var dataStorageAccountKey = StartupConfig.GetConfigurationVariable(config, "DATA_STORAGE_ACCOUNT_KEY");
            var sharedStorageContainerName = StartupConfig.GetConfigurationVariable(config, "SHARED_STORAGE_CONTAINER_NAME");
            var sharedStorageAccountName = StartupConfig.GetConfigurationVariable(config, "SHARED_STORAGE_ACCOUNT_NAME");
            var sharedStorageAccountKey = StartupConfig.GetConfigurationVariable(config, "SHARED_STORAGE_ACCOUNT_KEY");
            var timeSeriesPath = StartupConfig.GetConfigurationVariable(config, "TIME_SERIES_PATH");
            var meteringPointsPath = StartupConfig.GetConfigurationVariable(config, "METERING_POINTS_PATH");
            var marketRolesPath = StartupConfig.GetConfigurationVariable(config, "MARKET_ROLES_PATH");
            var chargesPath = StartupConfig.GetConfigurationVariable(config, "CHARGES_PATH");
            var chargeLinksPath = StartupConfig.GetConfigurationVariable(config, "CHARGE_LINKS_PATH");
            var chargePricesPath = StartupConfig.GetConfigurationVariable(config, "CHARGE_PRICES_PATH");
            var esBrpRelationsPath = StartupConfig.GetConfigurationVariable(config, "ES_BRP_RELATIONS_PATH");
            var gridLossSystemCorrectionPath = StartupConfig.GetConfigurationVariable(config, "GRID_LOSS_SYSTEM_CORRECTION_PATH");
            var snapshotPath = StartupConfig.GetConfigurationVariable(config, "SNAPSHOT_PATH");
            var resultUrl = new Uri(StartupConfig.GetConfigurationVariable(config, "RESULT_URL"));
            var snapshotUrl = new Uri(StartupConfig.GetConfigurationVariable(config, "SNAPSHOT_URL"));
            var aggregationPythonFile = StartupConfig.GetConfigurationVariable(config, "AGGREGATION_PYTHON_FILE");
            var wholesalePythonFile = StartupConfig.GetConfigurationVariable(config, "WHOLESALE_PYTHON_FILE");
            var dataPreparationPythonFile = StartupConfig.GetConfigurationVariable(config, "DATA_PREPARATION_PYTHON_FILE");
             */
        }

        /// <inheritdoc/>
        protected override Task OnFunctionAppHostFailedAsync(IReadOnlyList<string> hostLogSnapshot, Exception exception)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            return base.OnFunctionAppHostFailedAsync(hostLogSnapshot, exception);
        }

        /// <inheritdoc/>
        protected override async Task OnDisposeFunctionAppDependenciesAsync()
        {
            // => Storage
            AzuriteManager.Dispose();
        }

        private static string GetBuildConfiguration()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
    }
}
