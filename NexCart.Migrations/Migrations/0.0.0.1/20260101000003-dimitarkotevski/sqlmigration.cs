using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Create table dbo.Categories (baseline schema).
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20260101000003, "Create table dbo.Categories")]
    public sealed class Migration_20260101000003 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.1\20260101000003-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.1\20260101000003-dimitarkotevski\down.sql");
        }
    }
}