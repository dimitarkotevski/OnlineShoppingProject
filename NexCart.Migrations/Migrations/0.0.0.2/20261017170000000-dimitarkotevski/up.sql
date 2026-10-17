-- Migration: Harden dbo.Roles and dbo.Users.RoleId
--   * add NormalizedName (canonical, collation-independent role key) + unique index
--   * seed the built-in roles (Admin, User)
--   * make Users.RoleId required and default it to the least-privileged role
--
-- FluentMigrator sends the whole script as a single batch (it does not understand GO),
-- and SQL Server resolves column names when it compiles a batch. Statements that read or
-- write NormalizedName therefore run through EXEC(...) so they are compiled only after the
-- column exists.

-- 1) Add the canonical name column.
IF COL_LENGTH('dbo.Roles', 'NormalizedName') IS NULL
BEGIN
    ALTER TABLE dbo.Roles ADD NormalizedName NVARCHAR(255) NULL;
END

-- 2) Backfill any pre-existing rows (the table is normally empty).
EXEC(N'
UPDATE dbo.Roles
SET NormalizedName = UPPER(Name)
WHERE NormalizedName IS NULL;');

-- 3) Seed the built-in roles with fixed ids so down.sql can remove exactly these rows.
EXEC(N'
INSERT INTO dbo.Roles (Id, Name, NormalizedName)
SELECT v.Id, v.Name, v.NormalizedName
FROM (VALUES
    (CAST(''B0000000-0000-0000-0000-000000000001'' AS UNIQUEIDENTIFIER), N''Admin'', N''ADMIN''),
    (CAST(''B0000000-0000-0000-0000-000000000002'' AS UNIQUEIDENTIFIER), N''User'',  N''USER'')
) AS v (Id, Name, NormalizedName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.NormalizedName = v.NormalizedName);');

-- 4) Require a value and enforce uniqueness on it.
EXEC(N'
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID(''dbo.Roles'') AND name = ''NormalizedName'' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Roles ALTER COLUMN NormalizedName NVARCHAR(255) NOT NULL;
END');

EXEC(N'
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE object_id = OBJECT_ID(''dbo.Roles'') AND name = ''UX_Roles_NormalizedName'')
BEGIN
    CREATE UNIQUE INDEX UX_Roles_NormalizedName ON dbo.Roles (NormalizedName);
END');

-- 5) Give every existing user a role (default to the least-privileged one).
UPDATE dbo.Users
SET RoleId = (SELECT Id FROM dbo.Roles WHERE Name = N'User')
WHERE RoleId IS NULL;

-- 6) Require a role and default future inserts to the User role.
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'RoleId' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Users ALTER COLUMN RoleId UNIQUEIDENTIFIER NOT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints
               WHERE parent_object_id = OBJECT_ID('dbo.Users') AND name = 'DF_Users_RoleId')
BEGIN
    ALTER TABLE dbo.Users ADD CONSTRAINT DF_Users_RoleId
        DEFAULT 'B0000000-0000-0000-0000-000000000002' FOR RoleId;
END
