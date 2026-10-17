-- Migration: Create dbo.Brands and link dbo.Products to a Brand
-- Table: dbo.Brands (BrandMap.cs) / dbo.Products.BrandId
--
-- NOTE: FluentMigrator runs this file as a single batch (no GO support), so the
-- statement that references the freshly added Products.BrandId column is executed
-- through dynamic SQL to avoid a compile-time "Invalid column name" error.

-- ==========================================================================================
-- 1. BRANDS TABLE
-- ==========================================================================================
IF OBJECT_ID('dbo.Brands', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Brands (
        Id   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Brands PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(255)    NOT NULL,
        CONSTRAINT UQ_Brands_Name UNIQUE (Name)
    );
END;

-- ==========================================================================================
-- 2. LINK PRODUCTS -> BRANDS (one Brand has many Products)
-- ==========================================================================================
IF COL_LENGTH('dbo.Products', 'BrandId') IS NULL
BEGIN
    ALTER TABLE dbo.Products ADD BrandId UNIQUEIDENTIFIER NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_Brands')
BEGIN
    EXEC(N'ALTER TABLE dbo.Products
           ADD CONSTRAINT FK_Products_Brands FOREIGN KEY (BrandId) REFERENCES dbo.Brands (Id);');
END;

-- ==========================================================================================
-- 3. SEED BRANDS (electronics shop, grouped by the shop categories)
--    Idempotent: fixed Ids / names are only inserted when missing.
-- ==========================================================================================
INSERT INTO dbo.Brands (Id, Name)
SELECT v.Id, v.Name
FROM (VALUES
    -- Laptops / Tablets
    (CONVERT(UNIQUEIDENTIFIER, '114C2A3E-517D-4E8D-82D9-949C68EEBAFF'), N'Apple'),
    (CONVERT(UNIQUEIDENTIFIER, 'CD6EC66E-E4FE-4F62-B4F6-331251A3C142'), N'Dell'),
    (CONVERT(UNIQUEIDENTIFIER, 'BE405884-EE02-4CB3-8FDF-2DC1BDDF037B'), N'HP'),
    (CONVERT(UNIQUEIDENTIFIER, 'E84DB4BD-C3AA-4C01-91E0-21B6DEAAF325'), N'Lenovo'),
    (CONVERT(UNIQUEIDENTIFIER, '9D07D13A-A451-41A3-98BA-37F2E5BDFF74'), N'ASUS'),
    (CONVERT(UNIQUEIDENTIFIER, 'EB388EC5-0F6C-4B8A-BFF2-07711F06FC01'), N'Acer'),
    -- Smartphones / Tablets / Wearables / Home Electronics
    (CONVERT(UNIQUEIDENTIFIER, '4F5A7325-30BB-4397-9EA2-E61E6025C578'), N'Samsung'),
    (CONVERT(UNIQUEIDENTIFIER, '0D93A292-5A80-43C2-9F10-4AF064CB51FE'), N'Google'),
    (CONVERT(UNIQUEIDENTIFIER, '5F63021D-9F55-4F52-AF69-DB6B30F7DF19'), N'Xiaomi'),
    -- Headphones & Audio
    (CONVERT(UNIQUEIDENTIFIER, '39415C8E-C55B-4AB5-9DA7-21BF7E8C30D5'), N'Sony'),
    (CONVERT(UNIQUEIDENTIFIER, '7157EB33-0BEE-40AB-9097-D0CB89439208'), N'JBL'),
    (CONVERT(UNIQUEIDENTIFIER, 'AAB7E164-3FD7-4093-BB39-586BB8EA6550'), N'Bose'),
    (CONVERT(UNIQUEIDENTIFIER, '80D9DAB6-BAC5-4A5D-9B31-41E6BA27DE9A'), N'Sennheiser'),
    -- Smartwatches & Wearables
    (CONVERT(UNIQUEIDENTIFIER, 'F1257FF3-D43A-4C4B-B0FE-0D3CCD367204'), N'Garmin'),
    -- Computer Components
    (CONVERT(UNIQUEIDENTIFIER, '8D288D93-8AFE-46B0-8EC3-01C89D4A2E02'), N'Intel'),
    (CONVERT(UNIQUEIDENTIFIER, 'A987A65B-A3A2-4202-8C5C-A1EB75406BB5'), N'AMD'),
    (CONVERT(UNIQUEIDENTIFIER, 'EC8289B8-F30C-4EFB-86A6-82ADFA533E71'), N'NVIDIA'),
    (CONVERT(UNIQUEIDENTIFIER, '651EEEC0-A907-4389-9E56-489A7675970F'), N'Logitech'),
    -- Gaming Consoles
    (CONVERT(UNIQUEIDENTIFIER, '324BCC4F-0BAB-4B7F-9E4C-F445F5A0E314'), N'Microsoft'),
    (CONVERT(UNIQUEIDENTIFIER, '36293A1C-6792-419F-80CB-D64F1EEAE280'), N'Nintendo'),
    -- Cameras & Photography
    (CONVERT(UNIQUEIDENTIFIER, '00CD8E68-5327-4924-9889-DD66BA6892F2'), N'Canon'),
    (CONVERT(UNIQUEIDENTIFIER, 'FC9ADCCA-19BF-4151-89FD-21EEB8DABF58'), N'Nikon'),
    -- Home Electronics
    (CONVERT(UNIQUEIDENTIFIER, '5494A544-AF70-4B18-8FDA-AD5FB9B86388'), N'Philips'),
    (CONVERT(UNIQUEIDENTIFIER, '9B4D6200-BFA3-4ABA-8973-DF867D14FC1F'), N'LG'),
    -- Accessories
    (CONVERT(UNIQUEIDENTIFIER, '9B55774D-3670-45E2-967A-551F116503D0'), N'Anker')
) AS v (Id, Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Brands b WHERE b.Id = v.Id OR b.Name = v.Name);
