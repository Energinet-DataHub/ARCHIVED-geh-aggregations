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
    public static class StartupConfig
    {
        public static string GetConfigurationVariable(IConfiguration config, string key)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var variable = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrWhiteSpace(variable))
            {
                variable = config[$"Values:{key}"];
            }

            if (string.IsNullOrWhiteSpace(variable))
            {
                throw new Exception($"Missing app configuration: {key}");
            }

            return variable;
        }
    }
}
