-- Down migration: unlink Products from Brands and drop dbo.Brands
-- Runs as a single batch (FluentMigrator has no GO support), so the column drop is
-- issued through dynamic SQL to keep the guard independent of batch compilation.

-- 1. Drop the Products -> Brands foreign key
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_Brands')
BEGIN
    ALTER TABLE dbo.Products DROP CONSTRAINT FK_Products_Brands;
END;

-- 2. Drop the BrandId column (dropping a column drops its default constraint automatically)
IF COL_LENGTH('dbo.Products', 'BrandId') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.Products DROP COLUMN BrandId;');
END;

-- 3. Drop the Brands table (its unique constraint and seed rows go with it)
IF OBJECT_ID('dbo.Brands', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Brands;
END;
