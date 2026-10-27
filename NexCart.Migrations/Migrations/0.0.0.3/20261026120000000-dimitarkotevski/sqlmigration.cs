using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Adds immutable order line snapshots (dbo.OrderItems), converts money to DECIMAL(18,2),
    /// tightens the Orders columns, and fixes product/cart delete behaviour.
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20261026120000000, "Snapshot order items and use decimal money")]
    public sealed class Migration_20261026120000000 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.3\20261026120000000-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.3\20261026120000000-dimitarkotevski\down.sql");
        }
    }
}
