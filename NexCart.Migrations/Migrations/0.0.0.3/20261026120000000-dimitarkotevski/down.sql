-- ==========================================================================================
-- UNDO: SNAPSHOT ORDERS AND USE DECIMAL MONEY
--
-- OrderItems is dropped (the data cannot be represented by the old schema). FLOAT types and
-- the old permissive/nullable order columns are restored.
-- ==========================================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- 1. Remove the snapshot table.
IF OBJECT_ID('dbo.OrderItems', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.OrderItems;
END

-- 2. Remove constraints/indexes introduced by the up migration.
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = 'IX_Orders_UserId_DateCreated'
             AND object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    DROP INDEX IX_Orders_UserId_DateCreated ON dbo.Orders;
END

IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = 'UX_Orders_OnHoldShoppingCart'
             AND object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    DROP INDEX UX_Orders_OnHoldShoppingCart ON dbo.Orders;
END

IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = 'UX_Orders_OrderName'
             AND object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    DROP INDEX UX_Orders_OrderName ON dbo.Orders;
END

IF OBJECT_ID('dbo.CK_Users_Amount_NonNegative', 'C') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_Amount_NonNegative;
END

IF OBJECT_ID('dbo.CK_Products_Price_NonNegative', 'C') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Products DROP CONSTRAINT CK_Products_Price_NonNegative;
END

IF OBJECT_ID('dbo.CK_Orders_TotalAmount_NonNegative', 'C') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT CK_Orders_TotalAmount_NonNegative;
END

IF OBJECT_ID('dbo.CK_Orders_PaymentType', 'C') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT CK_Orders_PaymentType;
END

IF OBJECT_ID('dbo.CK_Orders_OrderStatus', 'C') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT CK_Orders_OrderStatus;
END

IF OBJECT_ID('dbo.CK_Orders_OrderName_NotBlank', 'C') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT CK_Orders_OrderName_NotBlank;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_Orders_DateCreated'
             AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT DF_Orders_DateCreated;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_Orders_PaymentType'
             AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT DF_Orders_PaymentType;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_Orders_OrderStatus'
             AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT DF_Orders_OrderStatus;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_Orders_TotalAmount'
             AND parent_object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT DF_Orders_TotalAmount;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_Users_Amount'
             AND parent_object_id = OBJECT_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT DF_Users_Amount;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_Products_Price'
             AND parent_object_id = OBJECT_ID('dbo.Products'))
BEGIN
    ALTER TABLE dbo.Products DROP CONSTRAINT DF_Products_Price;
END

-- 3. Restore the old foreign key behaviour.
DECLARE @fkName SYSNAME;
DECLARE @dropFkSql NVARCHAR(MAX);

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
        FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id);

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
        FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id);

SET @fkName = NULL;

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
        FOREIGN KEY (ShoppingCartId) REFERENCES dbo.ShoppingCarts (Id);

-- 4. Restore the old nullable types and data types.
ALTER TABLE dbo.Orders ALTER COLUMN UserId      UNIQUEIDENTIFIER NULL;
ALTER TABLE dbo.Orders ALTER COLUMN DateCreated DATETIME         NULL;
ALTER TABLE dbo.Orders ALTER COLUMN PaymentType INT              NULL;
ALTER TABLE dbo.Orders ALTER COLUMN OrderStatus INT              NULL;
ALTER TABLE dbo.Orders ALTER COLUMN OrderName   NVARCHAR(255)    NULL;
ALTER TABLE dbo.Orders ALTER COLUMN TotalAmount FLOAT            NULL;

ALTER TABLE dbo.Users ALTER COLUMN Amount FLOAT NULL;
ALTER TABLE dbo.Products ALTER COLUMN Price FLOAT NULL;

-- 5. Restore the original defaults.
ALTER TABLE dbo.Orders ADD CONSTRAINT DF_Orders_TotalAmount DEFAULT 0 FOR TotalAmount;
ALTER TABLE dbo.Orders ADD CONSTRAINT DF_Orders_DateCreated DEFAULT GETDATE() FOR DateCreated;
ALTER TABLE dbo.Products ADD CONSTRAINT DF_Products_Price DEFAULT 0 FOR Price;
ALTER TABLE dbo.Users ADD CONSTRAINT DF_Users_Amount DEFAULT 0 FOR Amount;
