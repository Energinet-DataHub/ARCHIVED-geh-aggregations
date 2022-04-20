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
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Aggregations.Common
{
    internal static class HealthCheckConfiguration
    {
        public static void AddHealthChecks(this IServiceCollection services, HostBuilderContext context)
        {
            //Health checks
            services.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
            services.AddHealthChecks()
                .AddLiveCheck()
                .AddSqlServer(
                    name: "MeteringPointMasterDataDb",
                    connectionString: context.Configuration[EnvironmentSettingNames.MasterDataDbConString])
                .AddMeteringPointItems(context);
        }

        public static IHealthChecksBuilder AddMeteringPointItems(this IHealthChecksBuilder services, HostBuilderContext context)
        {
            return services.AddAzureServiceBusTopic(
                name: "MeteringPointCreatedTopic",
                connectionString: context.Configuration[EnvironmentSettingNames.IntegrationEventListenerConnectionString],
                topicName: context.Configuration[EnvironmentSettingNames.MeteringPointCreatedTopicName])
                .AddAzureServiceBusSubscription(
                    name: "MeteringPointCreatedSubscription",
                    connectionString: context.Configuration[EnvironmentSettingNames.IntegrationEventListenerConnectionString],
                    subscriptionName: context.Configuration[EnvironmentSettingNames.MeteringPointCreatedSubscriptionName],
                    topicName: context.Configuration[EnvironmentSettingNames.MeteringPointCreatedTopicName])
                .AddAzureServiceBusTopic(
                    name: "MeteringPointConnectedTopic",
                    connectionString: context.Configuration[EnvironmentSettingNames.IntegrationEventListenerConnectionString],
                    topicName: context.Configuration[EnvironmentSettingNames.MeteringPointConnectedTopicName])
                .AddAzureServiceBusSubscription(
                    name: "MeteringPointConnectedSubscription",
                    connectionString: context.Configuration[EnvironmentSettingNames.IntegrationEventListenerConnectionString],
                    subscriptionName: context.Configuration[EnvironmentSettingNames.MeteringPointConnectedSubscriptionName],
                    topicName: context.Configuration[EnvironmentSettingNames.MeteringPointConnectedTopicName]);
        }
    }
}
