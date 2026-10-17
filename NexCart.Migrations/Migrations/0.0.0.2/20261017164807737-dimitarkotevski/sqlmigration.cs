using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Reset categories to electronics shop categories.
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20261017164807737, "Reset categories to electronics shop")]
    public sealed class Migration_20261017164807737 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017164807737-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017164807737-dimitarkotevski\down.sql");
        }
    }
}