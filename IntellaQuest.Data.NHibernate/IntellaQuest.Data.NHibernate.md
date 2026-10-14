# IntellaQuest.Data.NHibernate — Project Doc

> Assembly: **IntellaQuest.Data.NHibernate**. Data access layer: NHibernate + FluentNHibernate, generic Repository pattern, Unit of Work.

## Structure

```
ConfigurationRepository/   NHibernateContext, SessionFactoryProvider, Repository<T>, IUnitOfWork/UnitOfWork
Repositories/              Per-entity repositories
Mapping/                   FluentNHibernate ClassMap<T> per entity
Properties/                AssemblyInfo.cs
```

## Core infrastructure (`ConfigurationRepository/`)

**`NHibernateContext`**
- `BuildSessionFactory(string connectionString)` — FluentNHibernate config, MS SQL 2005 dialect, loads all `Mapping/*Map` classes from the assembly.
- Driver: `NHibernate.Driver.MicrosoftDataSqlClientDriver` over `Microsoft.Data.SqlClient` (`System.Data.SqlClient` is not available on .NET).
- Uses **NHibernate 5.5.x** + **FluentNHibernate 3.5.x** (≥ NHibernate 5.5.2 is required — earlier versions fail proxy validation on modern .NET with *"method MemberwiseClone should be 'public/protected virtual'"*).
- The connection string is passed in by the caller from application configuration — it is not hardcoded.
- Note: `SchemaMetadataUpdater` opens a connection at build time, so a reachable `ShopDb` is required even before the first query.

**`SessionFactoryProvider : ISessionFactoryProvider`** — builds the session factory lazily, exactly once, and reuses it for the lifetime of the application (`Get()`).

**`Repository<T> : NHibernateContext, IRepository<T>`** — generic CRUD:
`Add` / `Update` / `Delete` (entity, `IEnumerable`, or `Guid`), `All()`, `FindBy(Guid)`, `FindBy(expression)`, `FilterBy(expression)`, `CheckExist(expression)`. All operations go through `_unitOfWork.Session`.

**`IUnitOfWork` / `UnitOfWork`**
- `Session`, `BeginTransaction()`, `Commit()`, `Rollback()`, `Dispose()`.
- The constructor takes an `ISessionFactoryProvider`, so the session factory is built once and shared rather than per unit of work.

## Entity repositories (`Repositories/`)

| Repository | Extra methods beyond `IRepository<T>` |
|---|---|
| `UserRepository` | — |
| `RoleRepository` | `GetAdminRole()`, `GetUserRole()` |
| `ProductRepository` | — |
| `CategoryRepository` | `GetProductsByCategory(Category)` |
| `OrderRepository` | — |
| `PaymentRepository` | — |
| `ShoppingCartRepository` | — |
| `ShoppingCartDetailRepository` | — |
| `FavouriteProductsRepository` | `GetUserFavouriteProducts(User)` |

All take `IUnitOfWork` in the constructor. Registered in Autofac by name convention (`*Repository` → `AsImplementedInterfaces()`, `InstancePerLifetimeScope`) — see `IntellaQuest.Web/Program.cs`.

## Mappings (`Mapping/`)

One `ClassMap<T>` per entity (`UserMap`, `ProductMap`, `CategoryMap`, `OrderMap`, `PaymentMap`, `RoleMap`, `ShoppingCartMap`, `ShoppingCartDetailMap`, `FavouriteProductsMap`).

Conventions (see `UserMap`):
- `Schema("dbo")`, `Table("<PluralizedName>")` (e.g. `Users`)
- `Id(c => c.Id)`
- `.Access.CamelCaseField(Prefix.Underscore)` — maps to the private `_field` backing fields
- `.Not.Nullable()` on required columns, `.Nullable()` on optional

## Quirks

- `Repository<T>` inherits `NHibernateContext` (static-member inheritance — unusual but works).
- **SQL migrations:** `IntellaQuest.Migrations/` creates `ShopDb` and applies the schema via FluentMigrator (see its `README.md`).
