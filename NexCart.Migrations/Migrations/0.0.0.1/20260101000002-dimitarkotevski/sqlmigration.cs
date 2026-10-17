using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Create table dbo.Users (baseline schema).
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20260101000002, "Create table dbo.Users")]
    public sealed class Migration_20260101000002 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.1\20260101000002-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.1\20260101000002-dimitarkotevski\down.sql");
        }
    }
}