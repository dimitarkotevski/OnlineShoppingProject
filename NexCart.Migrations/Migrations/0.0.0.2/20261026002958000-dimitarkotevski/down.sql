-- ==========================================================================================
-- UNDO: HARDEN SHOPPING CART TABLES
--
-- This restores the original permissive schema (nullable columns, no cascade delete).
-- The data clean-up performed by up.sql (merged duplicate lines, deactivated duplicate
-- carts, deleted orphan rows) is intentionally not reversible.
-- ==========================================================================================

SET NOCOUNT ON;

-- 1. Indexes.
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ShoppingCartDetails_ProductId'
      AND object_id = OBJECT_ID('dbo.ShoppingCartDetails'))
BEGIN
    DROP INDEX IX_ShoppingCartDetails_ProductId ON dbo.ShoppingCartDetails;
END;

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_ShoppingCartDetails_CartProduct'
      AND object_id = OBJECT_ID('dbo.ShoppingCartDetails'))
BEGIN
    DROP INDEX UX_ShoppingCartDetails_CartProduct ON dbo.ShoppingCartDetails;
END;

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ShoppingCarts_UserId'
      AND object_id = OBJECT_ID('dbo.ShoppingCarts'))
BEGIN
    DROP INDEX IX_ShoppingCarts_UserId ON dbo.ShoppingCarts;
END;

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_ShoppingCarts_ActiveUser'
      AND object_id = OBJECT_ID('dbo.ShoppingCarts'))
BEGIN
    DROP INDEX UX_ShoppingCarts_ActiveUser ON dbo.ShoppingCarts;
END;

-- 2. Check constraints.
IF OBJECT_ID('dbo.CK_ShoppingCartDetails_Quantity', 'C') IS NOT NULL
BEGIN
    ALTER TABLE dbo.ShoppingCartDetails DROP CONSTRAINT CK_ShoppingCartDetails_Quantity;
END;

IF OBJECT_ID('dbo.CK_ShoppingCarts_Name_NotBlank', 'C') IS NOT NULL
BEGIN
    ALTER TABLE dbo.ShoppingCarts DROP CONSTRAINT CK_ShoppingCarts_Name_NotBlank;
END;

-- 3. Foreign keys: remove the cascade and restore the original no-action FKs.
IF OBJECT_ID('dbo.FK_ShoppingCartDetails_ShoppingCarts', 'F') IS NOT NULL
BEGIN
    ALTER TABLE dbo.ShoppingCartDetails DROP CONSTRAINT FK_ShoppingCartDetails_ShoppingCarts;
END;

IF OBJECT_ID('dbo.FK_ShoppingCartDetails_Products', 'F') IS NOT NULL
BEGIN
    ALTER TABLE dbo.ShoppingCartDetails DROP CONSTRAINT FK_ShoppingCartDetails_Products;
END;

ALTER TABLE dbo.ShoppingCartDetails
    ADD CONSTRAINT FK_ShoppingCartDetails_ShoppingCarts
        FOREIGN KEY (ShoppingCartId) REFERENCES dbo.ShoppingCarts (Id);

ALTER TABLE dbo.ShoppingCartDetails
    ADD CONSTRAINT FK_ShoppingCartDetails_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id);

-- 4. Original nullable columns.
ALTER TABLE dbo.ShoppingCartDetails ALTER COLUMN ProductId      UNIQUEIDENTIFIER NULL;
ALTER TABLE dbo.ShoppingCartDetails ALTER COLUMN ShoppingCartId UNIQUEIDENTIFIER NULL;
ALTER TABLE dbo.ShoppingCartDetails ALTER COLUMN Quantity       INT              NULL;

ALTER TABLE dbo.ShoppingCarts ALTER COLUMN UserId UNIQUEIDENTIFIER NULL;
ALTER TABLE dbo.ShoppingCarts ALTER COLUMN Active BIT              NULL;
ALTER TABLE dbo.ShoppingCarts ALTER COLUMN Name   NVARCHAR(255)    NULL;

-- 5. Audit timestamps.
IF OBJECT_ID('dbo.DF_ShoppingCartDetails_AddedAt', 'D') IS NOT NULL
BEGIN
    ALTER TABLE dbo.ShoppingCartDetails DROP CONSTRAINT DF_ShoppingCartDetails_AddedAt;
END;

IF COL_LENGTH('dbo.ShoppingCartDetails', 'AddedAt') IS NOT NULL
BEGIN
    ALTER TABLE dbo.ShoppingCartDetails DROP COLUMN AddedAt;
END;

IF OBJECT_ID('dbo.DF_ShoppingCarts_CreatedAt', 'D') IS NOT NULL
BEGIN
    ALTER TABLE dbo.ShoppingCarts DROP CONSTRAINT DF_ShoppingCarts_CreatedAt;
END;

IF COL_LENGTH('dbo.ShoppingCarts', 'CreatedAt') IS NOT NULL
BEGIN
    ALTER TABLE dbo.ShoppingCarts DROP COLUMN CreatedAt;
END;
