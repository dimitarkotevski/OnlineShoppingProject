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
- ⚠️ Connection string **hardcoded**: `Data Source=localhost;Initial Catalog=ShopDb;Integrated Security=SSPI;`

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

All take `IUnitOfWork` in the constructor. Registered in Autofac by name convention (`*Repository` → `AsImplementedInterfaces()`, `InstancePerRequest`) — see `IntellaQuest.Web/Global.asax.cs`.

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
- No migrations — schema is presumably created/managed manually against `ShopDb`.
