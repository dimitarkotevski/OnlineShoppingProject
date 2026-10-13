-- Migration: Order entity (IntellaQuest.Domain.Domains.Order)
-- Table: dbo.Orders (OrderMap.cs)
-- OrderStatus / PaymentType are stored as INT (the enum values).

IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Orders PRIMARY KEY DEFAULT NEWID(),
        OrderStatus   INT              NULL,
        PaymentType   INT              NULL,
        TotalAmount   FLOAT            NULL,
        DateCreated   DATETIME         NULL,
        OrderName     NVARCHAR(255)    NULL,
        ShoppingCartId UNIQUEIDENTIFIER NULL,
        UserId        UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_Orders_ShoppingCarts FOREIGN KEY (ShoppingCartId) REFERENCES dbo.ShoppingCarts (Id),
        CONSTRAINT FK_Orders_Users         FOREIGN KEY (UserId)         REFERENCES dbo.Users (Id)
    );
END
GO
