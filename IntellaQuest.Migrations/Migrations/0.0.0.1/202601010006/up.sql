-- Migration: ShoppingCartDetail entity (IntellaQuest.Domain.Domains.ShoppingCartDetail)
-- Table: dbo.ShoppingCartDetails (ShoppingCartDetailMap.cs)

IF OBJECT_ID('dbo.ShoppingCartDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShoppingCartDetails (
        Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ShoppingCartDetails PRIMARY KEY DEFAULT NEWID(),
        Quantity      INT              NULL,
        ShoppingCartId UNIQUEIDENTIFIER NULL,
        ProductId     UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_ShoppingCartDetails_ShoppingCarts FOREIGN KEY (ShoppingCartId) REFERENCES dbo.ShoppingCarts (Id),
        CONSTRAINT FK_ShoppingCartDetails_Products      FOREIGN KEY (ProductId)      REFERENCES dbo.Products (Id)
    );
END
GO
