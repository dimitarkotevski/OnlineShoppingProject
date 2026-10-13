using FluentMigrator;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Create table dbo.Orders (baseline schema).
    /// </summary>
    [Migration(20260101000007, "Create table dbo.Orders")]
    public sealed class Order_20260101000007 : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
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
");
        }

        public override void Down()
        {
            Execute.Sql("DROP TABLE IF EXISTS dbo.Orders;");
        }
    }
}