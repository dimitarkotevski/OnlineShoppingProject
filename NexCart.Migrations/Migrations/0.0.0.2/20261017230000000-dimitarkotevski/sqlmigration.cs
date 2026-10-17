using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Seeds the electronics shop product catalog across all categories.
    /// Resolves CategoryId and BrandId by name from dbo.Categories and dbo.Brands.
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20261017230000000, "Seed electronics shop products")]
    public sealed class Migration_20261017230000000 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017230000000-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017230000000-dimitarkotevski\down.sql");
        }
    }
}
