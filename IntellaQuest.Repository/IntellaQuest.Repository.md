# IntellaQuest.Repository — Project Doc

> Assembly: **IntellaQuest.Data.NHibernate**. Data access layer: NHibernate + FluentNHibernate, generic Repository pattern, Unit of Work.

## Structure

```
ConfigurationRepository/   NHibernateContext, Repository<T>, IUnitOfWork/UnitOfWork
Repositories/              Per-entity repositories
Mapping/                   FluentNHibernate ClassMap<T> per entity
Properties/                AssemblyInfo.cs
```

## Core infrastructure (`ConfigurationRepository/`)

**`NHibernateContext`**
- `BuildSessionFactory()` — FluentNHibernate config, MS SQL 2005 dialect, loads all `Mapping/*Map` classes from the assembly.
- Driver: `NHibernate.Driver.MicrosoftDataSqlClientDriver` over `Microsoft.Data.SqlClient` (`System.Data.SqlClient` is not available on .NET).
- Uses **NHibernate 5.5.x** + **FluentNHibernate 3.5.x** (≥ NHibernate 5.5.2 is required — earlier versions fail proxy validation on modern .NET with *"method MemberwiseClone should be 'public/protected virtual'"*).
- ⚠️ Connection string **hardcoded**: `Data Source=localhost;Initial Catalog=ShopDb;Integrated Security=SSPI;TrustServerCertificate=True;`
- Note: `SchemaMetadataUpdater` opens a connection at build time, so a reachable `ShopDb` is required even before the first query.

**`Repository<T> : NHibernateContext, IRepository<T>`** — generic CRUD:
`Add` / `Update` / `Delete` (entity, `IEnumerable`, or `Guid`), `All()`, `FindBy(Guid)`, `FindBy(expression)`, `FilterBy(expression)`, `CheckExist(expression)`. All operations go through `_unitOfWork.Session`.

**`IUnitOfWork` / `UnitOfWork`**
- `Session`, `BeginTransaction()`, `Commit()`, `Rollback()`, `Dispose()`.
- ⚠️ Constructor calls `NHibernateContext.BuildSessionFactory()` **every time** — a session factory per unit of work (expensive; should be a singleton).

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
- Namespaces are `IntellaQuest.Data.NHibernate.*` while the folder is `IntellaQuest.Repository/`.
- **SQL migrations:** `IntellaQuest.Migrations/` creates `ShopDb` and applies `001..009` scripts (see its `README.md`); the schema is otherwise managed by those scripts.
