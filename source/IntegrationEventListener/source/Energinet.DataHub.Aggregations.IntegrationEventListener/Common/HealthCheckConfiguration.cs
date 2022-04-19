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
                    connectionString: context.Configuration["DATABASE_MASTERDATA_CONNECTIONSTRING"])
                .AddMeteringPointItems(context);
        }

        public static IHealthChecksBuilder AddMeteringPointItems(this IHealthChecksBuilder services, HostBuilderContext context)
        {
            return services.AddAzureServiceBusTopic(
                name: "MeteringPointCreatedTopic",
                connectionString: context.Configuration["INTEGRATION_EVENT_MANAGER_CONNECTION_STRING"],
                topicName: context.Configuration["METERING_POINT_CREATED_TOPIC_NAME"])
                .AddAzureServiceBusSubscription(
                    name: "MeteringPointCreatedSubscription",
                    connectionString: context.Configuration["INTEGRATION_EVENT_MANAGER_CONNECTION_STRING"],
                    subscriptionName: context.Configuration["METERING_POINT_CREATED_SUBSCRIPTION_NAME"],
                    topicName: context.Configuration["METERING_POINT_CREATED_TOPIC_NAME"])
                .AddAzureServiceBusTopic(
                    name: "MeteringPointConnectedTopic",
                    connectionString: context.Configuration["INTEGRATION_EVENT_MANAGER_CONNECTION_STRING"],
                    topicName: context.Configuration["METERING_POINT_CONNECTED_TOPIC_NAME"])
                .AddAzureServiceBusSubscription(
                    name: "MeteringPointConnectedSubscription",
                    connectionString: context.Configuration["INTEGRATION_EVENT_MANAGER_CONNECTION_STRING"],
                    subscriptionName: context.Configuration["METERING_POINT_CONNECTED_SUBSCRIPTION_NAME"],
                    topicName: context.Configuration["METERING_POINT_CONNECTED_TOPIC_NAME"]);
        }
    }
}
