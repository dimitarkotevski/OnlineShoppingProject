using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Seed 10 categories into dbo.Categories.
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20261013230430, "Seed 10 categories")]
    public sealed class Migration_20261013230430 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.1\20261013230430-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.1\20261013230430-dimitarkotevski\down.sql");
        }
    }
}
