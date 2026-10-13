using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Create table dbo.Categories (baseline schema).
    /// </summary>
    [Migration(20260101000003, "Create table dbo.Categories")]
    public sealed class Category_20260101000003 : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
            -- Migration: Category entity (IntellaQuest.Domain.Domains.Category)
            -- Table: dbo.Categories (CategoryMap.cs)
            
            IF OBJECT_ID('dbo.Categories', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Categories (
                    Id     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Categories PRIMARY KEY DEFAULT NEWID(),
                    Name   NVARCHAR(255)    NOT NULL,
                    Status BIT              NOT NULL,
                    Url    NVARCHAR(255)    NOT NULL
                );
            END
");
        }

        public override void Down()
        {
            Execute.Sql("DROP TABLE IF EXISTS dbo.Categories;");
        }
    }
}