# IntellaQuest.Migrations

SQL migrations for the **ShopDb** database, built on
[FluentMigrator](https://fluentmigrator.github.io/). A small console app
(`Program.cs`) runs them; FluentMigrator tracks what has run in its own
`dbo.VersionInfo` table.

## Layout

```
IntellaQuest.Migrations/
    Program.cs              runner (up / down / list)
    appsettings.json        connection string + current version
    new-migration.tt        scaffolder (a T4 template)
    Migrations/
        Role_20260101000000.cs              baseline schema, inline SQL (one per table)
        Payment_20260101000001.cs
        ...
        FavouriteProducts_20260101000008.cs
        0.0.0.1/                            <- version (new migrations)
            20260113120000_kotev/           <- yyyyMMddHHmmss_gitUser
                1_class.cs                  <- FluentMigrator migration class
                up.sql                      <- CREATE
                down.sql                    <- REMOVE
```

Every migration is a class deriving from FluentMigrator's `Migration`, marked with
`[Migration(<id>, "<description>")]` and discovered automatically from the assembly —
nothing has to be registered.

## Baseline (inline)

The original per-entity schema (`001_Role.sql` … `009_FavouriteProducts.sql`) is defined
directly in C# as the nine `Migrations/*_2026010100000X.cs` classes. They use
`Execute.Sql(...)` and each statement is guarded, e.g.:

```csharp
[Migration(20260101000000, "Create table dbo.Roles")]
public sealed class Role_20260101000000 : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
        IF OBJECT_ID('dbo.Roles', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.Roles (
                Id   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Roles PRIMARY KEY DEFAULT NEWID(),
                Name NVARCHAR(255)    NOT NULL
            );
        END");
    }

    public override void Down() => Execute.Sql("DROP TABLE IF EXISTS dbo.Roles;");
}
```

Because each `Up()` is guarded, re-applying them to a database that already has the
tables is a no-op. The nine classes are ordered by their id (00 … 08) so foreign-key
targets exist before the tables that reference them.

## Creating a migration

New migrations are scaffolded with **`new-migration.tt`** (a T4 template — the same
idea as the template in the other project, trimmed down):

1. Open `new-migration.tt` in Visual Studio.
2. Select it and press **Ctrl+Shift+P** (*Run Custom Tool*), or right-click it →
   **Run Custom Tool**.

It creates one folder per migration:

```
Migrations\0.0.0.1\20260113120000_kotev\
    1_class.cs      the FluentMigrator migration class
    up.sql          CREATE   (run by "up")
    down.sql        REMOVE   (run by "down")
```

* `<version>` is `Migrations:Version` from `appsettings.json`.
* `<gitUser>` is `git config user.name` (falls back to your Windows user name).
* `<timestamp>` is the UTC time (`yyyyMMddHHmmss`); if that stamp is taken it bumps by
  a second.

The generated class wires the two SQL files up:

```csharp
[Migration(20260113120000, "20260113120000_kotev")]
public sealed class Migration_20260113120000_kotev : Migration
{
    public override void Up()
    {
        Execute.Script(@"Migrations\0.0.0.1\20260113120000_kotev\up.sql");
    }

    public override void Down()
    {
        Execute.Script(@"Migrations\0.0.0.1\20260113120000_kotev\down.sql");
    }
}
```

In SDK-style projects new files are picked up automatically, so there is no project
file to edit. (Running the template also drops a `new-migration.txt` next to it — that
is just the template's own output and is ignored by git.)

Put the **create** DDL in `up.sql` and the **remove/undo** SQL in `down.sql`.
`GO` is supported as a batch separator by the SQL Server processor.

## Version

The version folder lives in `appsettings.json`:

```json
{
  "ConnectionStrings": { "ShopDb": "..." },
  "Migrations": { "Version": "0.0.0.1" }
}
```

`new-migration.tt` reads it, so new migrations land under `Migrations\0.0.0.1\`. Bump it
when you start a new release; existing migrations keep their own folder and id and are
never re-run.

## Running

```powershell
# apply every pending migration (default)
dotnet run --project IntellaQuest.Migrations

# roll back the most recently applied migration (runs its Down)
dotnet run --project IntellaQuest.Migrations -- down

# show every migration and whether it is applied
dotnet run --project IntellaQuest.Migrations -- list

# explicit connection string (overrides appsettings.json)
dotnet run --project IntellaQuest.Migrations -- up "Data Source=.;Initial Catalog=ShopDb;Integrated Security=SSPI;TrustServerCertificate=True;"
```

**Visual Studio 2022:** set `IntellaQuest.Migrations` as the startup project and press
F5 / Ctrl+F5 (runs `up`). For `down` / `list`, pass the argument under
*Project Properties → Debug → Command line arguments*.

## How it works

1. Creates the database if it does not exist (default `ShopDb` on `localhost`, Windows
   auth). `CREATE DATABASE` runs against `master`, before FluentMigrator starts.
2. Reads the connection string from `ConnectionStrings:ShopDb` in `appsettings.json`
   (or the command-line argument, or the built-in default).
3. Builds a FluentMigrator DI container over `Microsoft.Data.SqlClient`, pointed at this
   assembly.
4. Runs `up` (`MigrateUp`), `down` (`Rollback(1)`) or `list` (`ListMigrations`).
   Applied migrations are recorded in `dbo.VersionInfo`.

`Execute.Script(...)` resolves its relative path against the working directory, so the
runner switches to the build output folder at start-up and `Migrations\**\*.sql` are
copied there on build
(`<None Update="Migrations\**\*.sql" CopyToOutputDirectory="PreserveNewest" />`).

## Notes

* Databases created with the old custom runner have an unused `dbo.__SqlMigrations`
  table; it can be dropped. FluentMigrator only knows about `dbo.VersionInfo`, so on
  such a database the nine baseline migrations run once more — harmless, because each
  is guarded by `IF OBJECT_ID(...) IS NULL`.
* **Do not change a migration's id, or the name/path of its SQL files, after it has been
  applied** — the id is what marks it as done.
