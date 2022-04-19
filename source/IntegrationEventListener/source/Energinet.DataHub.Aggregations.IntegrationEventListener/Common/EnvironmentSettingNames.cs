using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Energinet.DataHub.Aggregations.Common
{
    public static class EnvironmentSettingNames
    {
        public const string MeteringPointCreatedTopicName = "METERING_POINT_CREATED_TOPIC_NAME";
        public const string MeteringPointCreatedSubscriptionName = "METERING_POINT_CREATED_SUBSCRIPTION_NAME";
        public const string IntegrationEventListenerConnectionString = "INTEGRATION_EVENT_LISTENER_CONNECTION_STRING";
        public const string MeteringPointConnectedTopicName = "METERING_POINT_CONNECTED_TOPIC_NAME";
        public const string MeteringPointConnectedSubscriptionName = "METERING_POINT_CONNECTED_SUBSCRIPTION_NAME";
        public const string EnergySupplierChangedTopicName = "ENERGY_SUPPLIER_CHANGED_TOPIC_NAME";
        public const string EnergySupplierChangedSubscriptionName = "ENERGY_SUPPLIER_CHANGED_SUBSCRIPTION_NAME";
        public const string AppsettingsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public const string MasterDataDbConString = "DATABASE_MASTERDATA_CONNECTIONSTRING";
    }
}
