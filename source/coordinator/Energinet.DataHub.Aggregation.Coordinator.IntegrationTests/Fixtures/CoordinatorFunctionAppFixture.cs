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

namespace Energinet.DataHub.Aggregation.Coordinator.IntegrationTests.Fixtures
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
        protected override Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();
            return Task.CompletedTask;
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
            Environment.SetEnvironmentVariable("DATA_STORAGE_CONTAINER_NAME", "UNUSED");
            Environment.SetEnvironmentVariable("DATA_STORAGE_ACCOUNT_NAME", "UNUSED");
            Environment.SetEnvironmentVariable("DATA_STORAGE_ACCOUNT_KEY", "UNUSED");
            Environment.SetEnvironmentVariable("SHARED_STORAGE_CONTAINER_NAME", "UNUSED");
            Environment.SetEnvironmentVariable("SHARED_STORAGE_ACCOUNT_NAME", "UNUSED");
            Environment.SetEnvironmentVariable("SHARED_STORAGE_ACCOUNT_KEY", "UNUSED");
            Environment.SetEnvironmentVariable("TIME_SERIES_PATH", "UNUSED");
            Environment.SetEnvironmentVariable("METERING_POINTS_PATH", "UNUSED");
            Environment.SetEnvironmentVariable("MARKET_ROLES_PATH", "UNUSED");
            Environment.SetEnvironmentVariable("CHARGES_PATH", "UNUSED");
            Environment.SetEnvironmentVariable("CHARGE_LINKS_PATH", "UNUSED");
            Environment.SetEnvironmentVariable("CHARGE_PRICES_PATH", "UNUSED");
            Environment.SetEnvironmentVariable("CLUSTER_TIMEOUT_MINUTES", "1");
            Environment.SetEnvironmentVariable("DATABASE_CONNECTIONSTRING", "UNUSED");
            Environment.SetEnvironmentVariable("ES_BRP_RELATIONS_PATH", "UNUSED");
            Environment.SetEnvironmentVariable("GRID_LOSS_SYSTEM_CORRECTION_PATH", "UNUSED");
            Environment.SetEnvironmentVariable("SNAPSHOTS_BASE_PATH", "https://www.afakeurladress123.dk");
            Environment.SetEnvironmentVariable("RESULT_URL", "https://www.afakeurladress123.dk");
            Environment.SetEnvironmentVariable("SNAPSHOT_URL", "https://www.afakeurladress123.dk");
            Environment.SetEnvironmentVariable("AGGREGATION_PYTHON_FILE", "UNUSED");
            Environment.SetEnvironmentVariable("WHOLESALE_PYTHON_FILE", "UNUSED");
            Environment.SetEnvironmentVariable("DATA_PREPARATION_PYTHON_FILE", "UNUSED");
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
        protected override Task OnDisposeFunctionAppDependenciesAsync()
        {
            // => Storage
            AzuriteManager.Dispose();
            return Task.CompletedTask;
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
