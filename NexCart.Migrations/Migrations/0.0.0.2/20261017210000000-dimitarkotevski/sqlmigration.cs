using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Creates dbo.Brands and links dbo.Products to it (Products.BrandId -> Brands.Id),
    /// then seeds the common electronics brands sold across the shop categories.
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20261017210000000, "Create dbo.Brands, link Products.BrandId and seed brands")]
    public sealed class Migration_20261017210000000 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017210000000-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017210000000-dimitarkotevski\down.sql");
        }
    }
}
