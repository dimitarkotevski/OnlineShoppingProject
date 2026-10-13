using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Create table dbo.Payments (baseline schema).
    /// </summary>
    [Migration(20260101000001, "Create table dbo.Payments")]
    public sealed class Payment_20260101000001 : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
            -- Migration: Payment entity (IntellaQuest.Domain.Domains.Payment)
            -- Table: dbo.Payments (PaymentMap.cs)
            -- Note: column "BillingAdress" keeps the entity's spelling (renaming it needs a DB migration).
            
            IF OBJECT_ID('dbo.Payments', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Payments (
                    Id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Payments PRIMARY KEY DEFAULT NEWID(),
                    CardNumber     NVARCHAR(255)    NULL,
                    ExpirationDate DATETIME         NOT NULL,
                    SecurityCode   NVARCHAR(255)    NULL,
                    CardHolder     NVARCHAR(255)    NULL,
                    City           NVARCHAR(255)    NULL,
                    BillingAdress  NVARCHAR(255)    NULL,
                    ZipPostalCode  NVARCHAR(255)    NULL,
                    Country        NVARCHAR(255)    NULL,
                    PhoneNumber    NVARCHAR(255)    NULL
                );
            END
");
        }

        public override void Down()
        {
            Execute.Sql("DROP TABLE IF EXISTS dbo.Payments;");
        }
    }
}