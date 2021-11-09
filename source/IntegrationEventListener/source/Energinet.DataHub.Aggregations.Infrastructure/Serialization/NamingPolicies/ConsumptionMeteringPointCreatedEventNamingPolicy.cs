using System;
using System.Text.Json;

namespace Energinet.DataHub.Aggregations.Infrastructure.Serialization.NamingPolicies
{
    public class ConsumptionMeteringPointCreatedEventNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name switch
            {
                "MeteringPointId" => "metering_point_id",
                "MeteringPointType" => "metering_point_type",
                "GridArea" => "grid_area",
                "SettlementMethod" => "settlement_method",
                "MeteringMethod" => "metering_method",
                "Resolution" => "resolution",
                "Product" => "product",
                "ConnectionState" => "connection_state",
                "Unit" => "unit",
                "EffectiveDate" => "effective_date",
                "MessageVersion" => "MessageVersion",
                "MessageType" => "MessageType",
                "Transaction" => "Transaction",
                _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Could not convert property name.")
            };
        }
    }
}
