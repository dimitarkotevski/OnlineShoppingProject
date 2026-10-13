# IntellaQuest.Migrations

SQL migrations for the **ShopDb** database. A small console app (`Program.cs`) applies
them in order and records what ran, so each migration runs only once.

## Layout

```
IntellaQuest.Migrations/
    Program.cs              runner (up / down / list)
    Migration.cs            base class for folder migrations
    SqlScript.cs            splits scripts on GO
    new-migration.ps1       scaffolder (PowerShell)
    new-migration.cmd       scaffolder (double-click friendly wrapper)
    Migrations/
        001_Role.sql                          legacy flat script (forward only)
        ...
        009_FavouriteProducts.sql
        20261013120000_AddOrdersTable/        folder migration
            20261013120000_AddOrdersTable.cs
            up.sql
            down.sql
```

There are two kinds of migration and they can be mixed:

| Kind | Looks like | Up | Down |
|---|---|---|---|
| **Folder** | `Migrations\<id>_<Name>\` with a `Migration` class | `up.sql` | `down.sql` |
| **Legacy flat** | `Migrations\001_Role.sql` | the file | — (forward only) |

Ordering is by the numeric part of the name: legacy scripts use `001`…`009`, folder
migrations use the `yyyyMMddHHmmss` timestamp, so new ones always run after the old.

## Creating a migration

Run the scaffolder (pass a PascalCase name):

```powershell
.\new-migration.ps1 AddOrdersTable
# or, from Explorer / cmd:
new-migration.cmd AddOrdersTable
```

It creates a dated folder with three files:

```
Migrations\20261013120000_AddOrdersTable\
    20261013120000_AddOrdersTable.cs   <- Migration class (Id + Name), auto-discovered
    up.sql                             <- forward SQL   (run by "up")
    down.sql                           <- rollback SQL  (run by "down")
```

Put the forward DDL/DML in `up.sql` and the rollback in `down.sql`. `GO` is supported
as a batch separator. **Do not rename the folder or change `Id`/`Name` after the
migration has been applied** — that changes its recorded key and it would run again.

## Running

```powershell
# apply every pending migration (default)
dotnet run --project IntellaQuest.Migrations

# roll back the most recently applied migration
dotnet run --project IntellaQuest.Migrations -- down

# show every migration and whether it is applied
dotnet run --project IntellaQuest.Migrations -- list

# explicit connection string (also the default for the app)
dotnet run --project IntellaQuest.Migrations -- up "Data Source=.;Initial Catalog=ShopDb;Integrated Security=SSPI;TrustServerCertificate=True;"
```

**Visual Studio 2022:** set `IntellaQuest.Migrations` as the startup project and press
F5/Ctrl+F5 (runs `up`). For `down`/`list`, pass the argument under
*Project Properties → Debug → Command line arguments*.

## How it works

1. Creates the database if it doesn't exist (default `ShopDb` on `localhost`, Windows
   auth — same connection string as `NHibernateContext.cs`).
2. Creates the `dbo.__SqlMigrations` history table (`Name`, `AppliedOn`).
3. Discovers migrations:
   - every concrete `Migration` class in the assembly (folder migrations), and
   - every `Migrations\*.sql` file (legacy scripts);
4. sorts them by id and runs the pending ones **inside a transaction**, recording each
   in the history table.

`up.sql`/`down.sql` are copied to `bin\<config>\net10.0\Migrations\` on build
(`<None Update="Migrations\**\*.sql" CopyToOutputDirectory="PreserveNewest" />`).
