# IntellaQuest.Migrations

Database migrations for **ShopDb**, built on **FluentMigrator**. Each migration is a
small C# class (`sqlmigration.cs`) that runs a `up.sql` / `down.sql` script, and all
of them live under `Migrations\<version>\<timestamp>-<gitUser>\`.

`Program.cs` is the runner: on start it walks every migration in order and applies
the pending ones, so a single `dotnet run` creates the whole schema from scratch.

## Layout

```
IntellaQuest.Migrations/
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

namespace IntellaQuest.Migrations
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

## Creating a migration

New migrations are scaffolded with **`new-migration.tt`** (a T4 template):

1. Open `new-migration.tt` in Visual Studio.
2. Select it and press **Ctrl+Shift+P** (*Run Custom Tool*), or right-click it →
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
dotnet run --project IntellaQuest.Migrations

# roll back the most recently applied migration (runs its down.sql)
dotnet run --project IntellaQuest.Migrations -- down

# show every migration and whether it is applied
dotnet run --project IntellaQuest.Migrations -- list

# explicit connection string (overrides appsettings.json)
dotnet run --project IntellaQuest.Migrations -- up "Data Source=.;Initial Catalog=ShopDb;Integrated Security=SSPI;TrustServerCertificate=True;"
```

**Visual Studio 2022:** set `IntellaQuest.Migrations` as the startup project and
press F5 / Ctrl+F5 (runs `up`). For `down` / `list`, pass the argument under
*Project Properties → Debug → Command line arguments*.

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
