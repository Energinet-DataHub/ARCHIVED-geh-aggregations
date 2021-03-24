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
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Aggregation.Infrastructure
{
    public class StartupConfig
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

        public static string GetConfigurationVariableWithFallBack(string key, IConfiguration configuration)
        {
            var variable = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(variable))
            {
                variable = configuration.GetSection("Values").GetValue<string>(key);
                if (string.IsNullOrWhiteSpace(variable))
                {
                    throw new Exception($"Missing app configuration: {key}");
                }
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

        public static string GetCustomConnectionStringWithFallBack(string name, IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);
            }

            // Azure Functions App Service naming convention
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString =
                    Environment.GetEnvironmentVariable($"CUSTOMCONNSTR_{name}", EnvironmentVariableTarget.Process);
            }

            // Azure Functions App Service naming convention
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = configuration.GetSection("ConnectionStrings").GetValue<string>(name);
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception($"Can't find connection string ({name}) in configuration");
            }

            return connectionString!;
        }
    }
}
