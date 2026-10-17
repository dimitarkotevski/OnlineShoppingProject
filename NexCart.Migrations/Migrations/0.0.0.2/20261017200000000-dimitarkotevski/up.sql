-- Migration: replace the dbo.Roles lookup table + dbo.Users.RoleId FK with an INT Role column
--   * add Users.Role INT (Admin = 1, User = 2), matching NexCart.Domain.Enums.UserRole
--   * backfill it from the existing Roles rows
--   * require it, default it to User, and constrain it to the known roles
--   * drop the RoleId FK/column and the dbo.Roles table
--
-- FluentMigrator sends the whole script as a single batch (it does not understand GO), and
-- SQL Server resolves column names when it compiles a batch. Statements that touch the newly
-- added [Role] column therefore run through EXEC(...) so they compile only after it exists.

-- 1) Add the new column (nullable first so it can be backfilled).
IF COL_LENGTH('dbo.Users', 'Role') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD [Role] INT NULL;
END

-- 2) Backfill from the existing Roles table; anything unknown falls back to User (2).
EXEC(N'
UPDATE u
SET [Role] = CASE UPPER(r.NormalizedName)
                 WHEN N''ADMIN'' THEN 1
                 WHEN N''USER''  THEN 2
                 ELSE 2
             END
FROM dbo.Users u
LEFT JOIN dbo.Roles r ON r.Id = u.RoleId
WHERE u.[Role] IS NULL;');

-- 3) Default any remaining NULLs to User.
EXEC(N'UPDATE dbo.Users SET [Role] = 2 WHERE [Role] IS NULL;');

-- 4) Require the column.
EXEC(N'
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID(''dbo.Users'') AND name = ''Role'' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Users ALTER COLUMN [Role] INT NOT NULL;
END');

-- 5) Default future inserts to the least-privileged role and constrain to the enum values.
EXEC(N'
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints
               WHERE parent_object_id = OBJECT_ID(''dbo.Users'') AND name = ''DF_Users_Role'')
BEGIN
    ALTER TABLE dbo.Users ADD CONSTRAINT DF_Users_Role DEFAULT 2 FOR [Role];
END');

EXEC(N'
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE parent_object_id = OBJECT_ID(''dbo.Users'') AND name = ''CK_Users_Role'')
BEGIN
    ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Role CHECK ([Role] IN (1, 2));
END');

-- 6) Drop the old RoleId foreign key, default constraint and column.
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Roles')
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT FK_Users_Roles;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE parent_object_id = OBJECT_ID('dbo.Users') AND name = 'DF_Users_RoleId')
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT DF_Users_RoleId;
END

IF COL_LENGTH('dbo.Users', 'RoleId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Users DROP COLUMN RoleId;
END

-- 7) The lookup table is no longer used.
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Roles;
END
