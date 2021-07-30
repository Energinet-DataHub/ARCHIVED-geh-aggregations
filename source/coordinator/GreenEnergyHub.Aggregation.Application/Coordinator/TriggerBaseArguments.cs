using System.Collections.Generic;
using GreenEnergyHub.Aggregation.Application.Utilities;
using GreenEnergyHub.Aggregation.Infrastructure;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    public class TriggerBaseArguments : ITriggerBaseArguments
    {
        private readonly CoordinatorSettings _coordinatorSettings;

        public TriggerBaseArguments(CoordinatorSettings coordinatorSettings)
        {
            _coordinatorSettings = coordinatorSettings;
        }

        public List<string> GetTriggerBaseArguments(Instant beginTime, Instant endTime, string processType, bool persist)
        {
            return new List<string>
            {
                $"--data-storage-account-name={_coordinatorSettings.DataStorageAccountName}",
                $"--data-storage-account-key={_coordinatorSettings.DataStorageAccountKey}",
                $"--data-storage-container-name={_coordinatorSettings.DataStorageContainerName}",
                $"--time-series-path={_coordinatorSettings.TimeSeriesPath}",
                $"--beginning-date-time={beginTime.ToIso8601GeneralString()}",
                $"--end-date-time={endTime.ToIso8601GeneralString()}",
                $"--telemetry-instrumentation-key={_coordinatorSettings.TelemetryInstrumentationKey}",
                $"--process-type={processType}",
                $"--result-url={_coordinatorSettings.ResultUrl}?code={_coordinatorSettings.HostKey}",
                $"--snapshot-url={_coordinatorSettings.SnapshotUrl}?code={_coordinatorSettings.HostKey}",
                $"--persist-source-dataframe={persist}",
                $"--persist-source-dataframe-location={_coordinatorSettings.PersistLocation}",
                $"--cosmos-account-endpoint={_coordinatorSettings.CosmosAccountEndpoint}",
                $"--cosmos-account-key={_coordinatorSettings.CosmosAccountKey}",
                $"--cosmos-database={_coordinatorSettings.CosmosDatabase}",
                $"--cosmos-container-metering-points={_coordinatorSettings.CosmosContainerMeteringPoints}",
                $"--cosmos-container-market-roles={_coordinatorSettings.CosmosContainerMarketRoles}",
                $"--cosmos-container-grid-loss-sys-corr={_coordinatorSettings.CosmosContainerGridLossSysCorr}",
                $"--cosmos-container-es-brp-relations={_coordinatorSettings.CosmosContainerEsBrpRelations}",
            };
        }
    }
}
