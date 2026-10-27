# NexCart.Migrations

Database migrations for **ShopDb**, built on **FluentMigrator**. Each migration is a
small C# class (`sqlmigration.cs`) that runs a `up.sql` / `down.sql` script, and all
of them live under `Migrations\<version>\<timestamp>-<gitUser>\`.

`Program.cs` is the runner: on start it walks every migration in order and applies
the pending ones, so a single `dotnet run` creates the whole schema from scratch.

## Layout

```
NexCart.Migrations/
    Program.cs              runner (up / down / list)
    appsettings.json        connection string + current version
    new-migration.tt        scaffolder (a T4 template)
    Migrations/
        0.0.0.1/                            <- version (new migrations)
            20260101000000-dimitarkotevski/ <- <timestamp>-<gitUser>
                sqlmigration.cs             <- the FluentMigrator class
                up.sql                      <- CREATE  (run by "up")
                down.sql                    <- REMOVE  (run by "down")
            ...
            20260101000008-dimitarkotevski/
```

A migration lives in one folder, `Migrations\<version>\<timestamp>-<gitUser>\`,
where

* `<version>` is `Migrations:Version` from `appsettings.json`.
* `<timestamp>` is the UTC time (`yyyyMMddHHmmss`).
* `<gitUser>` is `git config user.name` (falls back to the Windows user name).

The folder holds exactly three files:

| File               | Purpose                                                          |
| ------------------ | ---------------------------------------------------------------- |
| `sqlmigration.cs`  | the FluentMigrator class (`[Migration(<timestamp>)]`)            |
| `up.sql`           | the **create** statements (the class runs it via `Execute.Script`)|
| `down.sql`         | the **remove/undo** statements                                    |

`sqlmigration.cs` is the migration itself:

```csharp
using FluentMigrator;

namespace NexCart.Migrations
{
    [Migration(20260101000000, "Create table dbo.Roles")]
    public sealed class Migration_20260101000000 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.1\20260101000000-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.1\20260101000000-dimitarkotevski\down.sql");
        }
    }
}
```

The `*.sql` files are copied next to the assembly on build
(`<None Update="Migrations\**\*.sql" CopyToOutputDirectory="PreserveNewest" />`),
which is where `Execute.Script` reads them from.

## Baseline

The nine baseline migrations create the original schema (`Roles`, `Payments`,
`Users`, `Categories`, `Products`, `ShoppingCarts`, `ShoppingCartDetails`,
`Orders`, `Favourite`). They are ordered by id (`…00` … `…08`) so foreign-key
targets exist before the tables that reference them, and each `up.sql` is guarded
with `IF OBJECT_ID(...) IS NULL`, so re-applying is a no-op.

## Database Schema (ShopDb)

> The built-in roles are the `NexCart.Domain.Enums.UserRole` enum. An early schema used a
> `dbo.Roles` lookup table + `Users.RoleId` FK; migration `20261017200000000` replaced it with
> an `INT` `Users.Role` column (`Admin = 1`, `User = 2`).

### Users
| Column | Type | Key/Constraint |
| :--- | :--- | :--- |
| `Id` | Guid | PK |
| `Username` | string | |
| `Password` | string | |
| `Email` | string | |
| `FirstName` | string | |
| `LastName` | string | |
| `Address` | string | |
| `City` | string | |
| `State` | string | |
| `ZipCode` | string | |
| `PhoneNumber` | string | |
| `Description` | string | |
| `Amount` | decimal(18,2) | NOT NULL, Default: 0, Check: `>= 0` |
| `Role` | int | Default: 2 (User); Check: `IN (1, 2)` — Admin = 1, User = 2 |
*Relationships: One-to-One with Payments; One-to-Many with Orders; Many-to-Many with Products via FavouriteProducts.*

#### Default administrator
Migration `20261017220000000` seeds a built-in administrator so a fresh database has an
account that can sign in and use the admin tool (`/admin-tool`, backed by the API's
`AdminOnly` policy):

| Username | Password | Role |
| :--- | :--- | :--- |
| `isoadmin` | `P@ssw0rd` | `Admin` (`UserRole.Admin`, stored as `Role = 1`) |

The password is stored as a PBKDF2-HMAC-SHA256 hash in the same format the app writes
(`NexCart.Business.Security.PasswordHasher`), so login verifies it like any other account.
**Change it after the first sign-in.** The row is inserted only when it does not already
exist, so re-applying the migration is a no-op.

### Categories
| Column | Type | Key/Constraint |
| :--- | :--- | :--- |
| `Id` | Guid | PK |
| `Name` | string | |
| `Url` | string | Default: '' |
| `Status` | bool | Default: 1 |
*Relationships: One-to-Many with Products.*

### Products
| Column | Type | Key/Constraint |
| :--- | :--- | :--- |
| `Id` | Guid | PK |
| `Name` | string | |
| `Price` | decimal(18,2) | NOT NULL, Default: 0, Check: `>= 0` |
| `ImageUrl` | string | |
| `Description` | string | |
| `Tags` | string | |
| `Created` | DateTime | Default: GETDATE() |
| `CategoryId` | Guid | FK $\rightarrow$ `Categories.Id` |
*Relationships: Many-to-One with Categories; One-to-Many with OrderItems; Many-to-Many with Users via FavouriteProducts.*

### ShoppingCarts
| Column | Type | Key/Constraint |
| :--- | :--- | :--- |
| `Id` | Guid | PK |
| `Name` | string(255) | NOT NULL, non-blank check |
| `Active` | bool | NOT NULL, Default: 1 |
| `UserId` | Guid | NOT NULL, FK $\rightarrow$ `Users.Id` |
| `CreatedAt` | DateTime2 | NOT NULL, Default: `SYSUTCDATETIME()` |
*Relationships: Many-to-One with Users; One-to-Many with ShoppingCartDetails.*
*Indexes: unique filtered `UX_ShoppingCarts_ActiveUser` on `UserId` where `Active = 1` (one active cart per user); `IX_ShoppingCarts_UserId`.*

### ShoppingCartDetails
| Column | Type | Key/Constraint |
| :--- | :--- | :--- |
| `Id` | Guid | PK |
| `Quantity` | int | NOT NULL, Default: 1, Check: `1`–`999` |
| `ShoppingCartId` | Guid | NOT NULL, FK $\rightarrow$ `ShoppingCarts.Id` (ON DELETE CASCADE) |
| `ProductId` | Guid | NOT NULL, FK $\rightarrow$ `Products.Id` (ON DELETE CASCADE) |
| `AddedAt` | DateTime2 | NOT NULL, Default: `SYSUTCDATETIME()` |
*Relationships: Many-to-One with ShoppingCarts; Many-to-One with Products.*
*Indexes: unique `UX_ShoppingCartDetails_CartProduct` on (`ShoppingCartId`, `ProductId`); `IX_ShoppingCartDetails_ProductId`.*

Migration `20261026002958000` applies this hardening. Before adding the constraints it merges
duplicate product lines, deactivates duplicate active carts (keeping the one with the most
lines) and removes orphan rows, so it is safe to run against an existing database.

### Orders
| Column | Type | Key/Constraint |
| :--- | :--- | :--- |
| `Id` | Guid | PK |
| `OrderName` | string(255) | NOT NULL, non-blank check, unique |
| `TotalAmount` | decimal(18,2) | NOT NULL, Default: 0, Check: `>= 0` |
| `DateCreated` | DateTime2 | NOT NULL, Default: `SYSUTCDATETIME()` |
| `PaymentType` | int | NOT NULL, Default: 0 (Cash), Check: `IN (0, 1, 2)` |
| `OrderStatus` | int | NOT NULL, Default: 1 (OnHold), Check: `IN (0, 1, 2, 3)` |
| `UserId` | Guid | NOT NULL, FK $\rightarrow$ `Users.Id` |
| `ShoppingCartId` | Guid | NULL, FK $\rightarrow$ `ShoppingCarts.Id` (ON DELETE SET NULL) |
*Relationships: Many-to-One with Users; optional Many-to-One with ShoppingCarts for traceability; One-to-Many with OrderItems.*
*Indexes: unique `UX_Orders_OrderName`; unique filtered `UX_Orders_OnHoldShoppingCart` on `ShoppingCartId` where `OrderStatus = 1` (one active order per cart); `IX_Orders_UserId_DateCreated` on (`UserId`, `DateCreated` DESC).*

### OrderItems
| Column | Type | Key/Constraint |
| :--- | :--- | :--- |
| `Id` | Guid | PK |
| `OrderId` | Guid | NOT NULL, FK $\rightarrow$ `Orders.Id` (ON DELETE CASCADE) |
| `ProductId` | Guid | NULL, FK $\rightarrow$ `Products.Id` (ON DELETE SET NULL) |
| `ProductName` | string(255) | NOT NULL — checkout-time snapshot |
| `UnitPrice` | decimal(18,2) | NOT NULL, Check: `>= 0` — checkout-time snapshot |
| `Quantity` | int | NOT NULL, Check: `> 0` |
*Relationships: Many-to-One with Orders; optional Many-to-One with Products.*
*Indexes: `IX_OrderItems_OrderId`; `IX_OrderItems_ProductId`.*

Order history reads these rows instead of the live cart/product catalogue, so later price, name,
cart or product changes cannot rewrite an order. Migration `20261026120000000` adds the table,
converts `Products.Price`, `Orders.TotalAmount` and `Users.Amount` from `FLOAT` to
`DECIMAL(18,2)`, backfills existing orders and tightens the `Orders` constraints.

### Payments
| Column | Type | Key/Constraint |
| :--- | :--- | :--- |
| `Id` | Guid | PK |
| `CardNumber` | string | |
| `CardHolder` | string | |
| `ExpirationDate` | DateTime | Default: GETDATE() |
| `City` | string | |
| `BillingAdress` | string | |
| `ZipPostalCode` | string | |
| `Country` | string | |
| `PhoneNumber` | string | |
*Relationships: One-to-One with Users.*

### FavouriteProducts
| Column | Type | Key/Constraint |
| :--- | :--- | :--- |
| `Id` | Guid | PK |
| `UserId` | Guid | FK $\rightarrow$ `Users.Id` |
| `ProductId` | Guid | FK $\rightarrow$ `Products.Id` (ON DELETE CASCADE) |
*Relationships: Many-to-One with Users; Many-to-One with Products.*

## Creating a migration

New migrations are scaffolded with **`new-migration.tt`** (a T4 template):

1. Open `new-migration.tt` in Visual Studio.
2. Select it and press **Ctrl+Shift+P** (*Run Custom Tool*), or right-click it $\rightarrow$
   **Run Custom Tool**.

It creates one folder per migration:

```
Migrations\0.0.0.1\20260113120000-dimitarkotevski\
    sqlmigration.cs
    up.sql
    down.sql
```

Put the **create** DDL in `up.sql` and the **remove/undo** SQL in `down.sql`; the
generated `sqlmigration.cs` already points at both.

In SDK-style projects new files are picked up automatically, so there is no
project file to edit. (Running the template also drops a `new-migration.txt` next
to it — that is just the template's own output and is ignored by git.)

## Version

The version folder lives in `appsettings.json`:

```json
{
  "ConnectionStrings": { "ShopDb": "..." },
  "Migrations": { "Version": "0.0.0.1" }
}
```

`new-migration.tt` reads it, so new migrations land under `Migrations\0.0.0.1\`.
Bump it when you start a new release; existing migrations keep their own folder
and are never re-run.

## Running

```powershell
# apply every pending migration (default) - creates the DB if missing
dotnet run --project NexCart.Migrations

# roll back the most recently applied migration (runs its down.sql)
dotnet run --project NexCart.Migrations -- down

# show every migration and whether it is applied
dotnet run --project NexCart.Migrations -- list

# explicit connection string (overrides appsettings.json)
dotnet run --project NexCart.Migrations -- up "Data Source=.;Initial Catalog=ShopDb;Integrated Security=SSPI;TrustServerCertificate=True;"
```

**Visual Studio 2022:** set `NexCart.Migrations` as the startup project and
press F5 / Ctrl+F5 (runs `up`). For `down` / `list`, pass the argument under
*Project Properties $\rightarrow$ Debug $\rightarrow$ Command line arguments*.

## How it works

1. Creates the target database if it does not exist (default `ShopDb` on
   `localhost`, Windows auth) — `CREATE DATABASE` runs against `master`.
2. Reads the connection string from `ConnectionStrings:ShopDb` in
   `appsettings.json` (or the command-line argument, or the built-in default).
3. Starts FluentMigrator, which discovers **every** `[Migration]` class in this
   assembly — the `sqlmigration.cs` files under `Migrations\<version>\...\` — and
   runs the pending ones in id order. It all runs from the output directory so
   `Execute.Script("...\up.sql")` finds the copied scripts.
4. Applied migrations are recorded by FluentMigrator itself in `dbo.VersionInfo`.
   Re-running applies only what is new.

## Notes

* Databases created by the even older flat-script runner may also hold an unused
  `dbo.__SqlMigrations` history; each baseline `up.sql` is guarded with
  `IF OBJECT_ID(...) IS NULL`, so a re-apply is harmless.
* **Do not reuse or change a migration's id once it has been applied** — the id is
  what marks it as done.
