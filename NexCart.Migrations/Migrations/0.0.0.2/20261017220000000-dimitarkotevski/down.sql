-- Down migration: remove the seeded administrator account.
-- Deletes only the account created by the matching up.sql (matched on the fixed Id + Username).
-- If the account has since created dependent rows (shopping carts, orders, favourites) the
-- delete is blocked by the foreign keys; clear those first for a full rollback.

DECLARE @AdminId UNIQUEIDENTIFIER = '983D06B2-9C00-4795-AB4B-2BEC9672C316';

DELETE FROM dbo.Users WHERE Id = @AdminId AND Username = N'isoadmin';
