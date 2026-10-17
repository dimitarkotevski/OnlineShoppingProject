using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Update Product Image from BLOB to String.
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20261017000001, "Update Product Image from BLOB to String")]
    public sealed class Migration_20261017000001 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017000001-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017000001-dimitarkotevski\down.sql");
        }
    }
}
