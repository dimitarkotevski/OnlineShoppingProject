-- Migration: replace the INT Users.Role column with the dbo.Roles lookup + RoleId FK (rollback)
--   * recreate dbo.Roles (Name + NormalizedName + unique index) and seed Admin/User
--   * recreate Users.RoleId, backfill it from the enum values, restore the FK + default
--   * drop the Users.Role enum column and its constraints
--
-- Same single-batch caveat as up.sql: statements that touch a just-(re)created column or table
-- run through EXEC(...) so they compile after the object exists.

-- 1) Recreate the Roles lookup table.
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        Id   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Roles PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(255)    NOT NULL
    );
END

-- 2) Add the canonical name column, backfill it, seed Admin/User, require + index it.
IF COL_LENGTH('dbo.Roles', 'NormalizedName') IS NULL
BEGIN
    ALTER TABLE dbo.Roles ADD NormalizedName NVARCHAR(255) NULL;
END

EXEC(N'
UPDATE dbo.Roles SET NormalizedName = UPPER(Name) WHERE NormalizedName IS NULL;');

EXEC(N'
INSERT INTO dbo.Roles (Id, Name, NormalizedName)
SELECT v.Id, v.Name, v.NormalizedName
FROM (VALUES
    (CAST(''B0000000-0000-0000-0000-000000000001'' AS UNIQUEIDENTIFIER), N''Admin'', N''ADMIN''),
    (CAST(''B0000000-0000-0000-0000-000000000002'' AS UNIQUEIDENTIFIER), N''User'',  N''USER'')
) AS v (Id, Name, NormalizedName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.NormalizedName = v.NormalizedName);');

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

-- 3) Recreate Users.RoleId (nullable first) and backfill it from the enum values.
IF COL_LENGTH('dbo.Users', 'RoleId') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD RoleId UNIQUEIDENTIFIER NULL;
END

EXEC(N'
UPDATE u
SET RoleId = CASE u.[Role]
                 WHEN 1 THEN (SELECT Id FROM dbo.Roles WHERE NormalizedName = N''ADMIN'')
                 ELSE          (SELECT Id FROM dbo.Roles WHERE NormalizedName = N''USER'')
             END
FROM dbo.Users u
WHERE u.RoleId IS NULL;');

-- 4) Restore the foreign key + default and require the column.
EXEC(N'
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = ''FK_Users_Roles'')
BEGIN
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id);
END');

EXEC(N'
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID(''dbo.Users'') AND name = ''RoleId'' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Users ALTER COLUMN RoleId UNIQUEIDENTIFIER NOT NULL;
END');

EXEC(N'
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints
               WHERE parent_object_id = OBJECT_ID(''dbo.Users'') AND name = ''DF_Users_RoleId'')
BEGIN
    ALTER TABLE dbo.Users ADD CONSTRAINT DF_Users_RoleId
        DEFAULT ''B0000000-0000-0000-0000-000000000002'' FOR RoleId;
END');

-- 5) Remove the enum column and its constraints.
IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE parent_object_id = OBJECT_ID('dbo.Users') AND name = 'CK_Users_Role')
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_Role;
END

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE parent_object_id = OBJECT_ID('dbo.Users') AND name = 'DF_Users_Role')
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT DF_Users_Role;
END

IF COL_LENGTH('dbo.Users', 'Role') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Users DROP COLUMN [Role];
END
