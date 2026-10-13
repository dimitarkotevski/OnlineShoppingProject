using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Create table dbo.Products (baseline schema).
    /// </summary>
    [Migration(20260101000004, "Create table dbo.Products")]
    public sealed class Product_20260101000004 : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
            -- Migration: Product entity (IntellaQuest.Domain.Domains.Product)
            -- Table: dbo.Products (ProductMap.cs)
            
            IF OBJECT_ID('dbo.Products', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Products (
                    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Products PRIMARY KEY DEFAULT NEWID(),
                    Name        NVARCHAR(255)    NULL,
                    Tags        NVARCHAR(MAX)    NULL,
                    Description NVARCHAR(MAX)    NULL,
                    Price       FLOAT            NULL,
                    Created     DATETIME         NULL,
                    Image       VARBINARY(MAX)   NULL,
                    CategoryId  UNIQUEIDENTIFIER NULL,
                    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories (Id)
                );
            END
");
        }

        public override void Down()
        {
            Execute.Sql("DROP TABLE IF EXISTS dbo.Products;");
        }
    }
}