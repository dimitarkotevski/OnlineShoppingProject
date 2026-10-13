-- Migration: Role entity (IntellaQuest.Domain.Domains.Role)
-- Table: dbo.Roles (RoleMap.cs)

IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        Id   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Roles PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(255)    NOT NULL
    );
END
GO
