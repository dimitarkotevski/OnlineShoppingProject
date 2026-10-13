# IntellaQuest.Migrations

SQL migrations for the **ShopDb** database. A small console app (`Program.cs`) applies
them in order and records what ran, so each migration runs only once.

## Layout

Migrations are grouped by **version**, then by the **datetime** they were created:

```
IntellaQuest.Migrations/
    Program.cs              runner (up / down / list)
    Migration.cs            base class (Version + Timestamp)
    SqlScript.cs            splits scripts on GO
    new-migration.ps1       scaffolder (PowerShell)
    new-migration.cmd       scaffolder (double-click friendly wrapper)
    Migrations/
        0.0.0.1/                            <- version
            202601010000/                   <- 2026-01-01 00:00 — db baseline (Roles)
                0.0.0.1_202601010000_Role.cs
                up.sql                      <- CREATE
                down.sql                    <- REMOVE
            ...
            202601010008/                   <- db baseline (FavouriteProducts)
            202610122030/                   <- 2026-10-12 20:30 — a later change
                0.0.0.1_202610122030_AddOrdersTable.cs
                up.sql
                down.sql
```

Each versioned migration folder holds:

| File | Meaning |
|---|---|
| `<Version>_<Timestamp>[_Name].cs` | the migration class (`Version` + `Timestamp`), auto-discovered |
| `up.sql` | the **create** script, run by `up` (the default) |
| `down.sql` | the **remove** script, run by `down` |

Ordering is by version, then timestamp. Legacy flat scripts (`Migrations\*.sql`) are
still supported and always run first (as version `0.0.0.0`), but the original `001`…`009`
scripts have been converted into the version/timestamp layout — see **Baseline** below.

## Baseline (`0.0.0.1`)

The nine original flat scripts (`001_Role.sql` … `009_FavouriteProducts.sql`) now live as
normal versioned migrations under `Migrations\0.0.0.1\<timestamp>\` (one folder each, in
dependency order, with a matching `down.sql` that drops the table). Each class also
declares its old file name via `LegacyKeys`:

```csharp
public override IEnumerable<string> LegacyKeys => new[] { "001_Role.sql" };
```

So a database that already recorded `001_Role.sql` is **not** re-run: the runner treats the
legacy name as "already applied" and rewrites the history row onto the canonical key
(`0.0.0.1/202601010000`). Fresh databases simply record the canonical key from the start.

## Creating a migration

```powershell
.\new-migration.ps1 -Version 0.0.0.1 -Name AddOrdersTable
# or positional, from cmd:
new-migration.cmd 0.0.0.1 AddOrdersTable
```

Produces:

```
Migrations\0.0.0.1\202610122030\
    0.0.0.1_202610122030_AddOrdersTable.cs
    up.sql
    down.sql
```

Options:

| Parameter | Default | Notes |
|---|---|---|
| `-Version` | *(required)* | version folder, e.g. `0.0.0.1` |
| `-Name` | *(none)* | optional label for the class/file name |
| `-Timestamp` | now | `yyyyMMddHHmm`; taken folders are bumped by a minute |
| `-Root` | script folder | project folder to scaffold into |

Put the **create** DDL/DML in `up.sql` and the **remove** (undo) SQL in `down.sql`.
`GO` is supported as a batch separator.

**Do not rename the version/timestamp folders or change `Version`/`Timestamp` after the
migration has been applied** — that changes its recorded key and it would run again.

## Running

```powershell
# apply every pending migration (default)
dotnet run --project IntellaQuest.Migrations

# roll back the most recently applied migration (runs its down.sql)
dotnet run --project IntellaQuest.Migrations -- down

# show every migration and whether it is applied
dotnet run --project IntellaQuest.Migrations -- list

# explicit connection string (also the app default)
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
   - every concrete `Migration` class in the assembly (keyed by `<Version>/<Timestamp>`), and
   - every `Migrations\*.sql` legacy script (keyed by file name);
   a migration recorded under a `LegacyKeys` name counts as applied and its history row is
   moved onto the canonical key;
4. sorts them (version, then timestamp) and runs the pending ones **inside a
   transaction**, recording each in the history table.

`up.sql`/`down.sql` are copied to `bin\<config>\net10.0\Migrations\` on build
(`<None Update="Migrations\**\*.sql" CopyToOutputDirectory="PreserveNewest" />`).
