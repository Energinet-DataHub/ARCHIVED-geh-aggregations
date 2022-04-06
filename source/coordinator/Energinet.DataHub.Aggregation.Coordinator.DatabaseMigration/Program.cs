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
using System.IO;
using System.Linq;
using System.Reflection;
using DbUp;
using DbUp.Helpers;
using DbUp.ScriptProviders;

namespace Energinet.DataHub.Aggregation.Coordinator.DatabaseMigration
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var connectionString =
                args.FirstOrDefault()
                ?? "Server=localhost;Database=Coordinator;Trusted_Connection=True;";

            Console.WriteLine("Start executing predeployment scripts...");
            var preDeploymentScriptsPath = Path.GetFullPath(Path.Combine(@$"{Assembly.GetExecutingAssembly().Location}", @"..\..\..\..\PreDeployScripts"));
            var preDeploymentScriptsExecutor =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(preDeploymentScriptsPath, new FileSystemScriptOptions
                    {
                        IncludeSubDirectories = true,
                    })
                    .LogToConsole()
                    .JournalTo(new NullJournal())
                    .Build();

            var preDeploymentUpgradeResult = preDeploymentScriptsExecutor.PerformUpgrade();

            if (!preDeploymentUpgradeResult.Successful)
            {
                return ReturnError(preDeploymentUpgradeResult.Error.ToString());
            }

            ShowSuccess();

            Console.WriteLine("Start executing migration scripts...");
            var migrationScriptsPath = Path.GetFullPath(Path.Combine(@$"{Assembly.GetExecutingAssembly().Location}", @"..\..\..\..\MigrationScripts"));
            var migrationScriptsExecuter =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(migrationScriptsPath, new FileSystemScriptOptions
                    {
                        IncludeSubDirectories = true,
                    })
                    .LogToConsole()
                    .Build();

            var migrationResult = migrationScriptsExecuter.PerformUpgrade();

            if (!migrationResult.Successful)
            {
                return ReturnError(migrationResult.Error.ToString());
            }

            ShowSuccess();

            return 0;
        }

        private static void ShowSuccess()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
        }

        private static int ReturnError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
            return -1;
        }
    }
}
