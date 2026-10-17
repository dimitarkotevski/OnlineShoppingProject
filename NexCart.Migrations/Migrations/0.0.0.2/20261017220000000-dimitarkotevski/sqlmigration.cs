using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Seeds the built-in administrator account ("isoadmin") so a fresh database has a user
    /// that can sign in to the admin tool (/admin-tool). The SQL lives in up.sql / down.sql
    /// next to this file.
    /// </summary>
    [Migration(20261017220000000, "Seed default administrator account (isoadmin)")]
    public sealed class Migration_20261017220000000 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017220000000-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017220000000-dimitarkotevski\down.sql");
        }
    }
}
