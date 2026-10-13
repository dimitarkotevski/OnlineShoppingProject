using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Create table dbo.Payments (baseline schema).
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20260101000001, "Create table dbo.Payments")]
    public sealed class Migration_20260101000001 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.1\20260101000001-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.1\20260101000001-dimitarkotevski\down.sql");
        }
    }
}