using CsvHelper.Configuration;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;

namespace GreenEnergyHub.Aggregation.TestData.Application.Parsers
{
    public sealed class MeteringPointMap : ClassMap<MeteringPoint>
    {
        public MeteringPointMap()
        {
            Map(mp => mp.MeteringPointId).Name("MeteringPointId");
            Map(mp => mp.MeteringPointType).Name("MarketEvaluationPointType");
            Map(mp => mp.SettlementMethod).Name("SettlementMethod");
            Map(mp => mp.MeteringGridArea).Name("GridArea");
            Map(mp => mp.ConnectionState).Name("ConnectionState");
            Map(mp => mp.MeterReadingPeriodicity).Name("Resolution");
            Map(mp => mp.FromDate).Name("FromDate");
            Map(mp => mp.ToDate).Name("ToDate");
        }
    }
}
