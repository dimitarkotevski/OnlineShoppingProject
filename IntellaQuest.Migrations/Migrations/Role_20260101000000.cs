using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Create table dbo.Roles (baseline schema).
    /// </summary>
    [Migration(20260101000000, "Create table dbo.Roles")]
    public sealed class Role_20260101000000 : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
            -- Migration: Role entity (IntellaQuest.Domain.Domains.Role)
            -- Table: dbo.Roles (RoleMap.cs)
            
            IF OBJECT_ID('dbo.Roles', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Roles (
                    Id   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Roles PRIMARY KEY DEFAULT NEWID(),
                    Name NVARCHAR(255)    NOT NULL
                );
            END
");
        }

        public override void Down()
        {
            Execute.Sql("DROP TABLE IF EXISTS dbo.Roles;");
        }
    }
}