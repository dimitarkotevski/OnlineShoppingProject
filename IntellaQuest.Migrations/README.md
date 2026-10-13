# IntellaQuest.Migrations

SQL migrations for the **ShopDb** database — one migration file per entity, in foreign-key dependency order:

| # | Migration | Table | Entity |
|---|---|---|---|
| 001 | `Role.sql` | `dbo.Roles` | `Role` |
| 002 | `Payment.sql` | `dbo.Payments` | `Payment` |
| 003 | `User.sql` | `dbo.Users` | `User` |
| 004 | `Category.sql` | `dbo.Categories` | `Category` |
| 005 | `Product.sql` | `dbo.Products` | `Product` |
| 006 | `ShoppingCart.sql` | `dbo.ShoppingCarts` | `ShoppingCart` |
| 007 | `ShoppingCartDetail.sql` | `dbo.ShoppingCartDetails` | `ShoppingCartDetail` |
| 008 | `Order.sql` | `dbo.Orders` | `Order` |
| 009 | `FavouriteProducts.sql` | `dbo.Favourite` | `FavouriteProducts` |

## How it works

`Program.cs` (the console app):

1. Creates the database if it doesn't exist (default: `ShopDb` on `localhost`, Windows auth — same connection string as `NHibernateContext.cs`).
2. Creates a `dbo.__SqlMigrations` history table.
3. Runs every `Migrations\*.sql` file in name order, each inside a transaction, and records it in the history table — so **each migration runs only once**. Re-running the app executes only new migrations.

The `.sql` files are copied to `bin\Debug\net10.0\Migrations\` on build.

## How to run

**Visual Studio 2022:** set `IntellaQuest.Migrations` as the startup project and press F5 (or Ctrl+F5).

**Command line:**

```powershell
# build
dotnet build IntellaQuest.Migrations

# run — uses the default ShopDb connection string
dotnet run --project IntellaQuest.Migrations

# run with an explicit connection string
dotnet run --project IntellaQuest.Migrations -- "Data Source=.;Initial Catalog=ShopDb;Integrated Security=SSPI;TrustServerCertificate=True;"
```

## Adding a new migration

Drop a new file into `Migrations\` with the next numeric prefix (e.g. `010_YourEntity.sql`) and run the app again. Keep `GO` batch separators if you need them — the runner splits on them.
