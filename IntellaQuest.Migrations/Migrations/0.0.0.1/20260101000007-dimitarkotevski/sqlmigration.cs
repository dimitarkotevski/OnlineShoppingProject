using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Create table dbo.Orders (baseline schema).
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20260101000007, "Create table dbo.Orders")]
    public sealed class Migration_20260101000007 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.1\20260101000007-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.1\20260101000007-dimitarkotevski\down.sql");
        }
    }
}