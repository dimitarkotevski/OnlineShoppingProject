-- Migration: Seed the default administrator account
-- Table: dbo.Users (UserMap.cs)
--
-- Creates the built-in administrator so a fresh database has an account that can sign in
-- and use the admin tool (Angular SPA at /admin-tool, backed by the "AdminOnly" API policy).
-- Role = 1 is NexCart.Domain.Enums.UserRole.Admin (see migration 20261017200000000).
--
-- Idempotent: the row is inserted only when neither the fixed Id nor the Username exists.
--
-- Password: P@ssw0rd -- stored as a PBKDF2-HMAC-SHA256 hash in the exact format the app
-- writes (NexCart.Business.Security.PasswordHasher: PBKDF2$<iterations>$<salt>$<hash>),
-- so login verifies it like any other account. Change it after the first sign-in.

DECLARE @AdminId  UNIQUEIDENTIFIER = '983D06B2-9C00-4795-AB4B-2BEC9672C316';
DECLARE @Password NVARCHAR(255)    = N'PBKDF2$210000$Je4Z8IsXGBka1V8Fdn98+Q==$89mAaw/FAU6pV+iKN5VVxXu+pkBSvt1A1LbTg6uJBZo=';

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @AdminId OR Username = N'isoadmin')
BEGIN
    INSERT INTO dbo.Users (Id, FirstName, LastName, Email, Username, Password, Role, Amount)
    VALUES (@AdminId, N'NexCart', N'Admin', N'isoadmin@nexcart.local', N'isoadmin', @Password, 1, 0);
END
