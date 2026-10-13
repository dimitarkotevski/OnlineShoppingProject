using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Create table dbo.Favourite (baseline schema).
    /// </summary>
    [Migration(20260101000008, "Create table dbo.Favourite")]
    public sealed class FavouriteProducts_20260101000008 : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
            -- Migration: FavouriteProducts entity (IntellaQuest.Domain.Domains.FavouriteProducts)
            -- Table: dbo.Favourite (FavouriteProductsMap.cs).
            -- The same table backs the User.FavouriteProducts many-to-many mapping in UserMap.cs.
            
            IF OBJECT_ID('dbo.Favourite', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Favourite (
                    Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Favourite PRIMARY KEY DEFAULT NEWID(),
                    UserId    UNIQUEIDENTIFIER NULL,
                    ProductId UNIQUEIDENTIFIER NULL,
                    CONSTRAINT FK_Favourite_Users   FOREIGN KEY (UserId)    REFERENCES dbo.Users (Id),
                    CONSTRAINT FK_Favourite_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id)
                );
            END
");
        }

        public override void Down()
        {
            Execute.Sql("DROP TABLE IF EXISTS dbo.Favourite;");
        }
    }
}