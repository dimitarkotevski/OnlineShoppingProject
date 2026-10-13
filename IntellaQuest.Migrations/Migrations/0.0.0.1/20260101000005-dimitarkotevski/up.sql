-- Migration: ShoppingCart entity (IntellaQuest.Domain.Domains.ShoppingCart)
-- Table: dbo.ShoppingCarts (ShoppingCartMap.cs)

IF OBJECT_ID('dbo.ShoppingCarts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShoppingCarts (
        Id     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ShoppingCarts PRIMARY KEY DEFAULT NEWID(),
        Name   NVARCHAR(255)    NULL,
        Active BIT              NULL,
        UserId UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_ShoppingCarts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );
END