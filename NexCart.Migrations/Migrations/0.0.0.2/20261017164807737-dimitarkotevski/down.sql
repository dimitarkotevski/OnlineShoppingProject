-- Down migration for electronics categories
-- Since this is a seed/reset migration, we can't easily restore the original random categories
-- but we can clear them again.
DELETE FROM dbo.Categories;