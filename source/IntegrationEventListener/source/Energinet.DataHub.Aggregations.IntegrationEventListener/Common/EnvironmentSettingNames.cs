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

namespace Energinet.DataHub.Aggregations.Common
{
    public static class EnvironmentSettingNames
    {
        public const string MeteringPointCreatedTopicName = "METERING_POINT_CREATED_TOPIC_NAME";
        public const string MeteringPointCreatedSubscriptionName = "METERING_POINT_CREATED_SUBSCRIPTION_NAME";
        public const string IntegrationEventListenerConnectionString = "INTEGRATION_EVENT_LISTENER_CONNECTION_STRING";
        public const string IntegrationEventManagerConnectionString = "INTEGRATION_EVENT_MANAGER_CONNECTION_STRING";
        public const string MeteringPointConnectedTopicName = "METERING_POINT_CONNECTED_TOPIC_NAME";
        public const string MeteringPointConnectedSubscriptionName = "METERING_POINT_CONNECTED_SUBSCRIPTION_NAME";
        public const string EnergySupplierChangedTopicName = "ENERGY_SUPPLIER_CHANGED_TOPIC_NAME";
        public const string EnergySupplierChangedSubscriptionName = "ENERGY_SUPPLIER_CHANGED_SUBSCRIPTION_NAME";
        public const string AppsettingsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public const string MasterDataDbConString = "DATABASE_MASTERDATA_CONNECTIONSTRING";
    }
}
