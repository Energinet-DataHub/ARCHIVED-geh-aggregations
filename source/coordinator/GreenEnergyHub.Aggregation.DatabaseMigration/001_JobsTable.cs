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
using FluentMigrator;

namespace GreenEnergyHub.Aggregation.DatabaseMigration
{
    [Migration(1)]
    public class JobsTable : Migration
    {
        public override void Up()
        {
            Create.Table("Jobs")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("DatabricksJobId").AsString()
                .WithColumn("State").AsString()
                .WithColumn("Created").AsDateTimeOffset()
                .WithColumn("Owner").AsString().WithDefaultValue("Unknown")
                .WithColumn("SnapshotPath").AsString().Nullable()
                .WithColumn("ProcessType").AsString();

            Create.Table("Results")
                .WithColumn("JobId").AsGuid().ForeignKey("Jobs", "Id")
                .WithColumn("Name").AsString()
                .WithColumn("Path").AsString();
        }

        public override void Down()
        {
            Delete.Table("Jobs");
            Delete.Table("Results");
        }
    }
}
