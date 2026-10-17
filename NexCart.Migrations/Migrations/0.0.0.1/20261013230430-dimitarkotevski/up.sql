-- Migration: Seed 10 categories (IntellaQuest.Domain.Domains.Category)
-- Table: dbo.Categories (CategoryMap.cs)
--
-- Fixed ids so this migration can be rolled back with down.sql.

INSERT INTO dbo.Categories (Id, Name, Status, Url)
SELECT v.Id, v.Name, v.Status, v.Url
FROM (VALUES
    (CAST('C0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER), N'Electronics',   1, N'electronics'),
    (CAST('C0000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER), N'Clothing',      1, N'clothing'),
    (CAST('C0000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER), N'Home & Garden', 1, N'home-garden'),
    (CAST('C0000000-0000-0000-0000-000000000004' AS UNIQUEIDENTIFIER), N'Books',         1, N'books'),
    (CAST('C0000000-0000-0000-0000-000000000005' AS UNIQUEIDENTIFIER), N'Sports',        1, N'sports'),
    (CAST('C0000000-0000-0000-0000-000000000006' AS UNIQUEIDENTIFIER), N'Toys',          1, N'toys'),
    (CAST('C0000000-0000-0000-0000-000000000007' AS UNIQUEIDENTIFIER), N'Beauty',        1, N'beauty'),
    (CAST('C0000000-0000-0000-0000-000000000008' AS UNIQUEIDENTIFIER), N'Automotive',    1, N'automotive'),
    (CAST('C0000000-0000-0000-0000-000000000009' AS UNIQUEIDENTIFIER), N'Grocery',       1, N'grocery'),
    (CAST('C0000000-0000-0000-0000-000000000010' AS UNIQUEIDENTIFIER), N'Music',         1, N'music')
) AS v (Id, Name, Status, Url)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories c WHERE c.Id = v.Id);
