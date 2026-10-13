using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Create table dbo.ShoppingCarts (baseline schema).
    /// </summary>
    [Migration(20260101000005, "Create table dbo.ShoppingCarts")]
    public sealed class ShoppingCart_20260101000005 : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
            -- Migration: ShoppingCart entity (IntellaQuest.Domain.Domains.ShoppingCart)
            -- Table: dbo.ShoppingCarts (ShoppingCartMap.cs)
            
            IF OBJECT_ID('dbo.ShoppingCarts', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ShoppingCarts (
                    Id    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ShoppingCarts PRIMARY KEY DEFAULT NEWID(),
                    Name  NVARCHAR(255)    NULL,
                    Active BIT             NULL,
                    UserId UNIQUEIDENTIFIER NULL,
                    CONSTRAINT FK_ShoppingCarts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
                );
            END
");
        }

        public override void Down()
        {
            Execute.Sql("DROP TABLE IF EXISTS dbo.ShoppingCarts;");
        }
    }
}