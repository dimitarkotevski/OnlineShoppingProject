-- Migration: Category entity (IntellaQuest.Domain.Domains.Category)
-- Table: dbo.Categories (CategoryMap.cs)

IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        Id     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Categories PRIMARY KEY DEFAULT NEWID(),
        Name   NVARCHAR(255)    NOT NULL,
        Status BIT              NOT NULL,
        Url    NVARCHAR(255)    NOT NULL
    );
END