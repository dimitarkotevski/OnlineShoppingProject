-- Migration: Update Product Image from BLOB to String (IntellaQuest.Domain.Domains.Product)
-- Table: dbo.Products

IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Products ALTER COLUMN Image NVARCHAR(500) NULL;
END