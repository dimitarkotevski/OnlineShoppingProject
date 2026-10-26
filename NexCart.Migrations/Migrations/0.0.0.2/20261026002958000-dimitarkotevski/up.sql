-- ==========================================================================================
-- HARDEN SHOPPING CART TABLES
--
-- The baseline tables allowed NULL for every key/business column, duplicate active carts
-- per user and duplicate product lines per cart. This migration normalises existing data
-- first, then adds the constraints/indexes that make those states impossible going forward.
--
-- NOTE: FluentMigrator executes this file as one batch (no GO support).
-- ==========================================================================================

SET NOCOUNT ON;

-- ------------------------------------------------------------------------------------------
-- 1. Remove rows that cannot satisfy the stricter foreign keys.
--    Cart details whose cart/product no longer exists, and carts without a user, are junk.
-- ------------------------------------------------------------------------------------------
DELETE d
FROM dbo.ShoppingCartDetails AS d
LEFT JOIN dbo.ShoppingCarts AS c ON c.Id = d.ShoppingCartId
LEFT JOIN dbo.Products AS p ON p.Id = d.ProductId
WHERE d.ShoppingCartId IS NULL
   OR d.ProductId IS NULL
   OR c.Id IS NULL
   OR p.Id IS NULL;

DELETE d
FROM dbo.ShoppingCartDetails AS d
INNER JOIN dbo.ShoppingCarts AS c ON c.Id = d.ShoppingCartId
LEFT JOIN dbo.Users AS u ON u.Id = c.UserId
WHERE c.UserId IS NULL
   OR u.Id IS NULL;

DELETE c
FROM dbo.ShoppingCarts AS c
LEFT JOIN dbo.Users AS u ON u.Id = c.UserId
WHERE c.UserId IS NULL
   OR u.Id IS NULL;

-- ------------------------------------------------------------------------------------------
-- 2. Normalise existing values so the NOT NULL / CHECK constraints can be added.
-- ------------------------------------------------------------------------------------------
UPDATE dbo.ShoppingCarts
SET Name = 'Cart#' + LEFT(REPLACE(CONVERT(NVARCHAR(36), Id), '-', ''), 8)
WHERE Name IS NULL
   OR LTRIM(RTRIM(Name)) = '';

UPDATE dbo.ShoppingCarts
SET Active = 1
WHERE Active IS NULL;

UPDATE dbo.ShoppingCartDetails
SET Quantity = 1
WHERE Quantity IS NULL
   OR Quantity < 1;

UPDATE dbo.ShoppingCartDetails
SET Quantity = 999
WHERE Quantity > 999;

-- ------------------------------------------------------------------------------------------
-- 3. Merge duplicate (cart, product) lines into one row per product.
--    The surviving row gets the summed quantity (capped at the UI/backend limit of 999).
-- ------------------------------------------------------------------------------------------
;WITH DuplicateLines AS
(
    SELECT Id,
           SUM(CONVERT(BIGINT, Quantity)) OVER (PARTITION BY ShoppingCartId, ProductId) AS TotalQuantity,
           ROW_NUMBER() OVER (PARTITION BY ShoppingCartId, ProductId ORDER BY Id) AS RowNumber
    FROM dbo.ShoppingCartDetails
)
UPDATE d
SET Quantity = CASE WHEN x.TotalQuantity > 999 THEN 999 ELSE CONVERT(INT, x.TotalQuantity) END
FROM dbo.ShoppingCartDetails AS d
INNER JOIN DuplicateLines AS x ON x.Id = d.Id
WHERE x.RowNumber = 1;

;WITH DuplicateLines AS
(
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY ShoppingCartId, ProductId ORDER BY Id) AS RowNumber
    FROM dbo.ShoppingCartDetails
)
DELETE d
FROM dbo.ShoppingCartDetails AS d
INNER JOIN DuplicateLines AS x ON x.Id = d.Id
WHERE x.RowNumber > 1;

-- ------------------------------------------------------------------------------------------
-- 4. Keep only one active cart per user. The cart with the most lines wins; the rest are
--    deactivated (not deleted) so their history is preserved.
-- ------------------------------------------------------------------------------------------
;WITH CartSizes AS
(
    SELECT ShoppingCartId, COUNT(*) AS DetailCount
    FROM dbo.ShoppingCartDetails
    GROUP BY ShoppingCartId
),
RankedCarts AS
(
    SELECT c.Id,
           ROW_NUMBER() OVER (
               PARTITION BY c.UserId
               ORDER BY ISNULL(s.DetailCount, 0) DESC, c.Id
           ) AS RowNumber
    FROM dbo.ShoppingCarts AS c
    LEFT JOIN CartSizes AS s ON s.ShoppingCartId = c.Id
    WHERE c.Active = 1
)
UPDATE c
SET Active = 0
FROM dbo.ShoppingCarts AS c
INNER JOIN RankedCarts AS r ON r.Id = c.Id
WHERE r.RowNumber > 1;

-- ------------------------------------------------------------------------------------------
-- 5. Audit timestamps. Unmapped by NHibernate: the defaults fill them on insert.
-- ------------------------------------------------------------------------------------------
IF COL_LENGTH('dbo.ShoppingCarts', 'CreatedAt') IS NULL
BEGIN
    ALTER TABLE dbo.ShoppingCarts
        ADD CreatedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_ShoppingCarts_CreatedAt DEFAULT SYSUTCDATETIME();
END;

IF COL_LENGTH('dbo.ShoppingCartDetails', 'AddedAt') IS NULL
BEGIN
    ALTER TABLE dbo.ShoppingCartDetails
        ADD AddedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_ShoppingCartDetails_AddedAt DEFAULT SYSUTCDATETIME();
END;

-- ------------------------------------------------------------------------------------------
-- 6. Required columns.
-- ------------------------------------------------------------------------------------------
ALTER TABLE dbo.ShoppingCarts ALTER COLUMN Name   NVARCHAR(255)    NOT NULL;
ALTER TABLE dbo.ShoppingCarts ALTER COLUMN Active BIT              NOT NULL;
ALTER TABLE dbo.ShoppingCarts ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;

ALTER TABLE dbo.ShoppingCartDetails ALTER COLUMN Quantity       INT              NOT NULL;
ALTER TABLE dbo.ShoppingCartDetails ALTER COLUMN ShoppingCartId UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE dbo.ShoppingCartDetails ALTER COLUMN ProductId      UNIQUEIDENTIFIER NOT NULL;

-- ------------------------------------------------------------------------------------------
-- 7. Foreign keys: deleting a cart removes its lines; a line cannot exist without its
--    cart or product. Existing names are dropped dynamically so this also works on
--    databases where the constraints were created with different names.
-- ------------------------------------------------------------------------------------------
DECLARE @constraintName SYSNAME;

SELECT @constraintName = fk.name
FROM sys.foreign_keys AS fk
WHERE fk.parent_object_id = OBJECT_ID('dbo.ShoppingCartDetails')
  AND fk.referenced_object_id = OBJECT_ID('dbo.ShoppingCarts');

IF @constraintName IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.ShoppingCartDetails DROP CONSTRAINT ' + @constraintName);
END;

SET @constraintName = NULL;

SELECT @constraintName = fk.name
FROM sys.foreign_keys AS fk
WHERE fk.parent_object_id = OBJECT_ID('dbo.ShoppingCartDetails')
  AND fk.referenced_object_id = OBJECT_ID('dbo.Products');

IF @constraintName IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.ShoppingCartDetails DROP CONSTRAINT ' + @constraintName);
END;

ALTER TABLE dbo.ShoppingCartDetails
    ADD CONSTRAINT FK_ShoppingCartDetails_ShoppingCarts
        FOREIGN KEY (ShoppingCartId) REFERENCES dbo.ShoppingCarts (Id) ON DELETE CASCADE;

ALTER TABLE dbo.ShoppingCartDetails
    ADD CONSTRAINT FK_ShoppingCartDetails_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id);

-- ------------------------------------------------------------------------------------------
-- 8. Check constraints.
-- ------------------------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_ShoppingCarts_Name_NotBlank'
      AND parent_object_id = OBJECT_ID('dbo.ShoppingCarts'))
BEGIN
    ALTER TABLE dbo.ShoppingCarts WITH CHECK
        ADD CONSTRAINT CK_ShoppingCarts_Name_NotBlank
            CHECK (LEN(LTRIM(RTRIM(Name))) > 0);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_ShoppingCartDetails_Quantity'
      AND parent_object_id = OBJECT_ID('dbo.ShoppingCartDetails'))
BEGIN
    ALTER TABLE dbo.ShoppingCartDetails WITH CHECK
        ADD CONSTRAINT CK_ShoppingCartDetails_Quantity
            CHECK (Quantity BETWEEN 1 AND 999);
END;

-- ------------------------------------------------------------------------------------------
-- 9. Indexes:
--    * one active cart per user (the service always looks up the user's active cart)
--    * one line per product per cart
--    * product lookups ("is this product in a cart?")
-- ------------------------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_ShoppingCarts_ActiveUser'
      AND object_id = OBJECT_ID('dbo.ShoppingCarts'))
BEGIN
    CREATE UNIQUE INDEX UX_ShoppingCarts_ActiveUser
        ON dbo.ShoppingCarts (UserId)
        WHERE Active = 1;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ShoppingCarts_UserId'
      AND object_id = OBJECT_ID('dbo.ShoppingCarts'))
BEGIN
    CREATE INDEX IX_ShoppingCarts_UserId
        ON dbo.ShoppingCarts (UserId);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_ShoppingCartDetails_CartProduct'
      AND object_id = OBJECT_ID('dbo.ShoppingCartDetails'))
BEGIN
    CREATE UNIQUE INDEX UX_ShoppingCartDetails_CartProduct
        ON dbo.ShoppingCartDetails (ShoppingCartId, ProductId);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ShoppingCartDetails_ProductId'
      AND object_id = OBJECT_ID('dbo.ShoppingCartDetails'))
BEGIN
    CREATE INDEX IX_ShoppingCartDetails_ProductId
        ON dbo.ShoppingCartDetails (ProductId)
        INCLUDE (ShoppingCartId, Quantity);
END;
