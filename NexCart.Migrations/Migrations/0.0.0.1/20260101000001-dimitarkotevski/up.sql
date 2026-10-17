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