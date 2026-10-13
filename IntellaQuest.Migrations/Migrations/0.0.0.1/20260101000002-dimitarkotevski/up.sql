-- Migration: User entity (IntellaQuest.Domain.Domains.User)
-- Table: dbo.Users (UserMap.cs)

IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY DEFAULT NEWID(),
        FirstName   NVARCHAR(255)    NOT NULL,
        LastName    NVARCHAR(255)    NOT NULL,
        Email       NVARCHAR(255)    NOT NULL,
        Username    NVARCHAR(255)    NOT NULL,
        Password    NVARCHAR(255)    NOT NULL,
        Description NVARCHAR(MAX)    NULL,
        Address     NVARCHAR(255)    NULL,
        State       NVARCHAR(255)    NULL,
        City        NVARCHAR(255)    NULL,
        ZipCode     NVARCHAR(255)    NULL,
        Amount      FLOAT            NULL,
        PaymentId   UNIQUEIDENTIFIER NULL,
        RoleId      UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_Users_Payments FOREIGN KEY (PaymentId) REFERENCES dbo.Payments (Id),
        CONSTRAINT FK_Users_Roles    FOREIGN KEY (RoleId)    REFERENCES dbo.Roles (Id)
    );
END