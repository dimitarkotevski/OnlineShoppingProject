-- ==========================================================================================
-- GLOBAL SCHEMA DEFAULTS
-- Ensures non-nullable columns have sensible defaults to prevent "Cannot insert NULL" errors.
-- ==========================================================================================

-- 1. Boolean Statuses
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Categories') AND col_name(parent_object_id, parent_column_id) = 'Status')
    ALTER TABLE dbo.Categories ADD CONSTRAINT DF_Categories_Status DEFAULT 1 FOR Status;

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.ShoppingCarts') AND col_name(parent_object_id, parent_column_id) = 'Active')
    ALTER TABLE dbo.ShoppingCarts ADD CONSTRAINT DF_ShoppingCarts_Active DEFAULT 1 FOR Active;

-- 2. Numeric/Monetary Values
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Orders') AND col_name(parent_object_id, parent_column_id) = 'TotalAmount')
    ALTER TABLE dbo.Orders ADD CONSTRAINT DF_Orders_TotalAmount DEFAULT 0 FOR TotalAmount;

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Products') AND col_name(parent_object_id, parent_column_id) = 'Price')
    ALTER TABLE dbo.Products ADD CONSTRAINT DF_Products_Price DEFAULT 0 FOR Price;

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Users') AND col_name(parent_object_id, parent_column_id) = 'Amount')
    ALTER TABLE dbo.Users ADD CONSTRAINT DF_Users_Amount DEFAULT 0 FOR Amount;

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.ShoppingCartDetails') AND col_name(parent_object_id, parent_column_id) = 'Quantity')
    ALTER TABLE dbo.ShoppingCartDetails ADD CONSTRAINT DF_ShoppingCartDetails_Quantity DEFAULT 1 FOR Quantity;

-- 3. Timestamps
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Orders') AND col_name(parent_object_id, parent_column_id) = 'DateCreated')
    ALTER TABLE dbo.Orders ADD CONSTRAINT DF_Orders_DateCreated DEFAULT GETDATE() FOR DateCreated;

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Products') AND col_name(parent_object_id, parent_column_id) = 'Created')
    ALTER TABLE dbo.Products ADD CONSTRAINT DF_Products_Created DEFAULT GETDATE() FOR Created;

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Payments') AND col_name(parent_object_id, parent_column_id) = 'ExpirationDate')
    ALTER TABLE dbo.Payments ADD CONSTRAINT DF_Payments_ExpirationDate DEFAULT GETDATE() FOR ExpirationDate;

-- 4. Strings (Specific to Categories reset)
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Categories') AND col_name(parent_object_id, parent_column_id) = 'Url')
    ALTER TABLE dbo.Categories ADD CONSTRAINT DF_Categories_Url DEFAULT '' FOR Url;

-- ==========================================================================================
-- DATA RESET: ELECTRONICS SHOP
-- ==========================================================================================

-- Clear old categories
DELETE FROM dbo.Categories;

-- Insert new electronics categories
INSERT INTO dbo.Categories (Name, Url) VALUES ('Laptops', 'laptops');
INSERT INTO dbo.Categories (Name, Url) VALUES ('Smartphones', 'smartphones');
INSERT INTO dbo.Categories (Name, Url) VALUES ('Headphones & Audio', 'headphones-audio');
INSERT INTO dbo.Categories (Name, Url) VALUES ('Smartwatches & Wearables', 'wearables');
INSERT INTO dbo.Categories (Name, Url) VALUES ('Computer Components', 'components');
INSERT INTO dbo.Categories (Name, Url) VALUES ('Gaming Consoles', 'consoles');
INSERT INTO dbo.Categories (Name, Url) VALUES ('Cameras & Photography', 'cameras');
INSERT INTO dbo.Categories (Name, Url) VALUES ('Home Electronics', 'home-electronics');
INSERT INTO dbo.Categories (Name, Url) VALUES ('Tablets', 'tablets');
INSERT INTO dbo.Categories (Name, Url) VALUES ('Accessories', 'accessories');