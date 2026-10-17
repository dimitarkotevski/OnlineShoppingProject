-- Migration: Harden dbo.Roles and dbo.Users.RoleId (rollback)

-- 1) Drop the RoleId default and make the column nullable again.
IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE parent_object_id = OBJECT_ID('dbo.Users') AND name = 'DF_Users_RoleId')
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT DF_Users_RoleId;
END

IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'RoleId' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.Users ALTER COLUMN RoleId UNIQUEIDENTIFIER NULL;
END

-- 2) Remove the seeded roles (clear the references first).
UPDATE dbo.Users
SET RoleId = NULL
WHERE RoleId IN ('B0000000-0000-0000-0000-000000000001', 'B0000000-0000-0000-0000-000000000002');

DELETE FROM dbo.Roles
WHERE Id IN ('B0000000-0000-0000-0000-000000000001', 'B0000000-0000-0000-0000-000000000002');

-- 3) Drop the unique index and the NormalizedName column.
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE object_id = OBJECT_ID('dbo.Roles') AND name = 'UX_Roles_NormalizedName')
BEGIN
    DROP INDEX UX_Roles_NormalizedName ON dbo.Roles;
END

IF COL_LENGTH('dbo.Roles', 'NormalizedName') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Roles DROP COLUMN NormalizedName;
END
