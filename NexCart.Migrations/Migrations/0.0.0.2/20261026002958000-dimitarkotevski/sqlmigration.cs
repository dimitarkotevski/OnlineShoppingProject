using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Tightens the shopping cart schema: required user/product/cart keys, one active
    /// cart per user, one line per product per cart, valid quantities, cascade delete
    /// for cart lines and audit timestamps.
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20261026002958000, "Harden shopping cart tables")]
    public sealed class Migration_20261026002958000 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261026002958000-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261026002958000-dimitarkotevski\down.sql");
        }
    }
}
