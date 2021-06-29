using System;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure
{
    public static class StartupConfig
    {
        public static string GetConfigurationVariable(string key)
        {
            var variable = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(variable))
            {
                throw new Exception($"Missing app configuration: {key}");
            }

            return variable;
        }

        public static string GetCustomConnectionString(string name)
        {
            var connectionString = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString =
                    Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);
            }

            // Azure Functions App Service naming convention
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable($"CUSTOMCONNSTR_{name}", EnvironmentVariableTarget.Process);
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception($"Can't find connection string ({name}) in configuration");
            }

            return connectionString!;
        }
    }
}
