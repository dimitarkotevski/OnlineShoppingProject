using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Security hardening: clear stored card security codes (CVV) and stop persisting them.
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20261014090000, "Security hardening: remove stored card security codes (CVV)")]
    public sealed class Migration_20261014090000 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.1\20261014090000-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.1\20261014090000-dimitarkotevski\down.sql");
        }
    }
}
