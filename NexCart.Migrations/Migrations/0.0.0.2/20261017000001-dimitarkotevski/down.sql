-- Migration: Revert Product Image from String to BLOB (IntellaQuest.Domain.Domains.Product)
-- Table: dbo.Products

IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Products ALTER COLUMN Image VARBINARY(MAX) NULL;
END