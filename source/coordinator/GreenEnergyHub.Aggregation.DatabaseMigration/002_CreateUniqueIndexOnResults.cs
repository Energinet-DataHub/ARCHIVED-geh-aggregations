using FluentMigrator;

namespace GreenEnergyHub.Aggregation.DatabaseMigration
{
    [Migration(2)]
    public class CreateUniqueIndexOnResults : Migration
    {
        public override void Up()
        {
            Create.UniqueConstraint("UQ_Results_JobId_Name").OnTable("Results").Columns("JobId", "Name");
        }

        public override void Down()
        {
            Delete.UniqueConstraint("UQ_Results_JobId_Name");
        }
    }
}
