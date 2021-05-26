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
