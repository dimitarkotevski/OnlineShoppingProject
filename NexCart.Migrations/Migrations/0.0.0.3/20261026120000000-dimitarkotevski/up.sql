-- ==========================================================================================
-- SNAPSHOT ORDERS AND USE DECIMAL MONEY
--
-- Problems this fixes:
--   * Orders only referenced a mutable shopping cart, so later cart/product changes rewrote
--     order history and totals.
--   * Prices, order totals and user balances were stored as FLOAT.
--   * Orders had nullable business columns and no order number uniqueness.
--   * Deleting a product was blocked by every cart it had ever been in, while order history
--     depended on the cart still existing.
--
-- Existing order history is copied into dbo.OrderItems using the current product name/price,
-- because the old schema never stored a checkout-time snapshot. The stored total is preserved
-- for orders that have no cart lines; orders with lines are recalculated from the copied items.
--
-- FluentMigrator executes this file as one batch (no GO support), so statements that touch
-- newly added columns are executed through EXEC(...) to defer name resolution.
-- ==========================================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- ------------------------------------------------------------------------------------------
-- 1. Normalise order rows that cannot satisfy the stricter constraints.
-- ------------------------------------------------------------------------------------------
UPDATE o
SET UserId = c.UserId
FROM dbo.Orders AS o
INNER JOIN dbo.ShoppingCarts AS c ON c.Id = o.ShoppingCartId
WHERE o.UserId IS NULL
  AND c.UserId IS NOT NULL;

DELETE o
FROM dbo.Orders AS o
LEFT JOIN dbo.Users AS u ON u.Id = o.UserId
WHERE o.UserId IS NULL
   OR u.Id IS NULL;

UPDATE dbo.Orders
SET OrderName = 'Order#' + LEFT(REPLACE(CONVERT(NVARCHAR(36), Id), '-', ''), 8)
WHERE OrderName IS NULL
   OR LTRIM(RTRIM(OrderName)) = '';

UPDATE dbo.Orders SET OrderStatus = 1 WHERE OrderStatus IS NULL; -- OnHold
UPDATE dbo.Orders SET PaymentType = 0 WHERE PaymentType IS NULL; -- Cash
UPDATE dbo.Orders SET TotalAmount = 0 WHERE TotalAmount IS NULL;
UPDATE dbo.Orders SET TotalAmount = 0 WHERE TotalAmount < 0;
UPDATE dbo.Orders SET DateCreated = SYSUTCDATETIME() WHERE DateCreated IS NULL;

-- Keep only the newest active order per cart before enforcing that rule with a unique index.
;WITH DuplicateActiveOrders AS
(
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY ShoppingCartId ORDER BY DateCreated DESC, Id DESC) AS RowNumber
    FROM dbo.Orders
    WHERE OrderStatus = 1
      AND ShoppingCartId IS NOT NULL
)
DELETE o
FROM dbo.Orders AS o
INNER JOIN DuplicateActiveOrders AS d ON d.Id = o.Id
WHERE d.RowNumber > 1;

-- OrderName becomes unique. Suffix duplicate names with part of the row id.
;WITH DuplicateOrderNames AS
(
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY OrderName ORDER BY DateCreated, Id) AS RowNumber
    FROM dbo.Orders
)
UPDATE o
SET OrderName = LEFT(o.OrderName, 235) + N'#' + LEFT(REPLACE(CONVERT(NVARCHAR(36), o.Id), '-', ''), 8)
FROM dbo.Orders AS o
INNER JOIN DuplicateOrderNames AS d ON d.Id = o.Id
WHERE d.RowNumber > 1;

-- ------------------------------------------------------------------------------------------
-- 2. Decimal money. FLOAT is approximate and must never be used for prices or payments.
--    Default constraints are dropped/recreated so the type can be changed safely.
-- ------------------------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_Products_Price'
             AND parent_object_id = OBJECT_ID('dbo.Products'))
BEGIN
    ALTER TABLE dbo.Products DROP CONSTRAINT DF_Products_Price;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_Users_Amount'
             AND parent_object_id = OBJECT_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT DF_Users_Amount;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_Orders_TotalAmount'
             AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT DF_Orders_TotalAmount;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_Orders_DateCreated'
             AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT DF_Orders_DateCreated;
END

UPDATE dbo.Products SET Price = 0 WHERE Price IS NULL OR Price < 0;
UPDATE dbo.Users SET Amount = 0 WHERE Amount IS NULL OR Amount < 0;

ALTER TABLE dbo.Products ALTER COLUMN Price DECIMAL(18,2) NOT NULL;
ALTER TABLE dbo.Users ALTER COLUMN Amount DECIMAL(18,2) NOT NULL;
ALTER TABLE dbo.Orders ALTER COLUMN TotalAmount DECIMAL(18,2) NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints
               WHERE name = 'DF_Products_Price'
                 AND parent_object_id = OBJECT_ID('dbo.Products'))
BEGIN
    ALTER TABLE dbo.Products ADD CONSTRAINT DF_Products_Price DEFAULT 0 FOR Price;
END

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints
               WHERE name = 'DF_Users_Amount'
                 AND parent_object_id = OBJECT_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users ADD CONSTRAINT DF_Users_Amount DEFAULT 0 FOR Amount;
END

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints
               WHERE name = 'DF_Orders_TotalAmount'
                 AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders ADD CONSTRAINT DF_Orders_TotalAmount DEFAULT 0 FOR TotalAmount;
END

-- ------------------------------------------------------------------------------------------
-- 3. Immutable order lines.
-- ------------------------------------------------------------------------------------------
IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OrderItems PRIMARY KEY DEFAULT NEWID(),
        OrderId     UNIQUEIDENTIFIER NOT NULL,
        ProductId   UNIQUEIDENTIFIER NULL,
        ProductName NVARCHAR(255)    NOT NULL,
        UnitPrice   DECIMAL(18,2)    NOT NULL,
        Quantity    INT              NOT NULL,
        CONSTRAINT FK_OrderItems_Orders
            FOREIGN KEY (OrderId) REFERENCES dbo.Orders (Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Products
            FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id) ON DELETE SET NULL,
        CONSTRAINT CK_OrderItems_UnitPrice CHECK (UnitPrice >= 0),
        CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0)
    );
END

-- Copy the best available historical snapshot. The old schema had no checkout-time price or
-- name, so current catalogue values are used here.
INSERT INTO dbo.OrderItems (Id, OrderId, ProductId, ProductName, UnitPrice, Quantity)
SELECT NEWID(),
       o.Id,
       d.ProductId,
       ISNULL(p.Name, N''),
       p.Price,
       d.Quantity
FROM dbo.Orders AS o
INNER JOIN dbo.ShoppingCartDetails AS d ON d.ShoppingCartId = o.ShoppingCartId
INNER JOIN dbo.Products AS p ON p.Id = d.ProductId
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.OrderItems AS oi
    WHERE oi.OrderId = o.Id
);

UPDATE o
SET TotalAmount = itemTotals.TotalAmount
FROM dbo.Orders AS o
INNER JOIN (
    SELECT OrderId, SUM(UnitPrice * Quantity) AS TotalAmount
    FROM dbo.OrderItems
    GROUP BY OrderId
) AS itemTotals ON itemTotals.OrderId = o.Id;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_OrderItems_OrderId'
                 AND object_id = OBJECT_ID('dbo.OrderItems'))
BEGIN
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems (OrderId);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_OrderItems_ProductId'
                 AND object_id = OBJECT_ID('dbo.OrderItems'))
BEGIN
    CREATE INDEX IX_OrderItems_ProductId ON dbo.OrderItems (ProductId);
END

-- ------------------------------------------------------------------------------------------
-- 4. Required order columns and defaults.
-- ------------------------------------------------------------------------------------------
ALTER TABLE dbo.Orders ALTER COLUMN OrderName   NVARCHAR(255) NOT NULL;
ALTER TABLE dbo.Orders ALTER COLUMN OrderStatus INT           NOT NULL;
ALTER TABLE dbo.Orders ALTER COLUMN PaymentType INT           NOT NULL;
ALTER TABLE dbo.Orders ALTER COLUMN DateCreated DATETIME2(0)  NOT NULL;
ALTER TABLE dbo.Orders ALTER COLUMN UserId      UNIQUEIDENTIFIER NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints
               WHERE name = 'DF_Orders_OrderStatus'
                 AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders ADD CONSTRAINT DF_Orders_OrderStatus DEFAULT 1 FOR OrderStatus;
END

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints
               WHERE name = 'DF_Orders_PaymentType'
                 AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders ADD CONSTRAINT DF_Orders_PaymentType DEFAULT 0 FOR PaymentType;
END

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints
               WHERE name = 'DF_Orders_DateCreated'
                 AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders ADD CONSTRAINT DF_Orders_DateCreated DEFAULT SYSUTCDATETIME() FOR DateCreated;
END

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = 'CK_Orders_OrderName_NotBlank'
                 AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders WITH CHECK
        ADD CONSTRAINT CK_Orders_OrderName_NotBlank
            CHECK (LEN(LTRIM(RTRIM(OrderName))) > 0);
END

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = 'CK_Orders_OrderStatus'
                 AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders WITH CHECK
        ADD CONSTRAINT CK_Orders_OrderStatus
            CHECK (OrderStatus IN (0, 1, 2, 3));
END

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = 'CK_Orders_PaymentType'
                 AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders WITH CHECK
        ADD CONSTRAINT CK_Orders_PaymentType
            CHECK (PaymentType IN (0, 1, 2));
END

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = 'CK_Orders_TotalAmount_NonNegative'
                 AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders WITH CHECK
        ADD CONSTRAINT CK_Orders_TotalAmount_NonNegative
            CHECK (TotalAmount >= 0);
END

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = 'CK_Products_Price_NonNegative'
                 AND parent_object_id = OBJECT_ID('dbo.Products'))
BEGIN
    ALTER TABLE dbo.Products WITH CHECK
        ADD CONSTRAINT CK_Products_Price_NonNegative
            CHECK (Price >= 0);
END

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = 'CK_Users_Amount_NonNegative'
                 AND parent_object_id = OBJECT_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users WITH CHECK
        ADD CONSTRAINT CK_Users_Amount_NonNegative
            CHECK (Amount >= 0);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_Orders_OrderName'
                 AND object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    CREATE UNIQUE INDEX UX_Orders_OrderName ON dbo.Orders (OrderName);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_Orders_OnHoldShoppingCart'
                 AND object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    CREATE UNIQUE INDEX UX_Orders_OnHoldShoppingCart
        ON dbo.Orders (ShoppingCartId)
        WHERE OrderStatus = 1
          AND ShoppingCartId IS NOT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_Orders_UserId_DateCreated'
                 AND object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    CREATE INDEX IX_Orders_UserId_DateCreated ON dbo.Orders (UserId, DateCreated DESC);
END

-- ------------------------------------------------------------------------------------------
-- 5. Delete behaviour.
--    * Deleting a cart clears the traceability link but keeps the order snapshot.
--    * Deleting a product removes it from live carts/favourites, while order lines survive
--      with ProductId = NULL and their name/price snapshot intact.
-- ------------------------------------------------------------------------------------------
DECLARE @fkName SYSNAME;
DECLARE @dropFkSql NVARCHAR(MAX);

SELECT @fkName = fk.name
FROM sys.foreign_keys AS fk
WHERE fk.parent_object_id = OBJECT_ID('dbo.Orders')
  AND fk.referenced_object_id = OBJECT_ID('dbo.ShoppingCarts');

IF @fkName IS NOT NULL
BEGIN
    SET @dropFkSql = N'ALTER TABLE dbo.Orders DROP CONSTRAINT ' + QUOTENAME(@fkName);
    EXEC sp_executesql @dropFkSql;
END

ALTER TABLE dbo.Orders
    ADD CONSTRAINT FK_Orders_ShoppingCarts
        FOREIGN KEY (ShoppingCartId) REFERENCES dbo.ShoppingCarts (Id) ON DELETE SET NULL;

SET @fkName = NULL;

SELECT @fkName = fk.name
FROM sys.foreign_keys AS fk
WHERE fk.parent_object_id = OBJECT_ID('dbo.ShoppingCartDetails')
  AND fk.referenced_object_id = OBJECT_ID('dbo.Products');

IF @fkName IS NOT NULL
BEGIN
    SET @dropFkSql = N'ALTER TABLE dbo.ShoppingCartDetails DROP CONSTRAINT ' + QUOTENAME(@fkName);
    EXEC sp_executesql @dropFkSql;
END

ALTER TABLE dbo.ShoppingCartDetails
    ADD CONSTRAINT FK_ShoppingCartDetails_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id) ON DELETE CASCADE;

SET @fkName = NULL;

SELECT @fkName = fk.name
FROM sys.foreign_keys AS fk
WHERE fk.parent_object_id = OBJECT_ID('dbo.Favourite')
  AND fk.referenced_object_id = OBJECT_ID('dbo.Products');

IF @fkName IS NOT NULL
BEGIN
    SET @dropFkSql = N'ALTER TABLE dbo.Favourite DROP CONSTRAINT ' + QUOTENAME(@fkName);
    EXEC sp_executesql @dropFkSql;
END

ALTER TABLE dbo.Favourite
    ADD CONSTRAINT FK_Favourite_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id) ON DELETE CASCADE;
