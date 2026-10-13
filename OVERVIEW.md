# IntellaQuest Online Shopping Project — Overview

> Context file for AI assistants. Full-stack online shopping web app.
> Last updated: 2026-10-13

## Overview

**IntellaQuest** — an online shopping web site with an ASP.NET Core backend and an Angular SPA frontend.

- **Backend:** .NET 10 / ASP.NET Core (MVC + attribute-routed API controllers), NHibernate + FluentNHibernate (`Microsoft.Data.SqlClient` driver), SQL Server, Autofac DI, FluentValidation
- **Frontend:** Angular 20 SPA (lives inside `IntellaQuest.Web/my-app/`), Angular Material, Bootstrap 5, @auth0/angular-jwt
- **Solution:** `IntellaQuestProject.sln` (5 projects)

## Solution Structure

Each project also has its own detailed doc inside its folder:

| Project (assembly) | Folder | Doc |
|---|---|---|
| `IntellaQuest.Domain` | `IntellaQuest.Domain/` | [`IntellaQuest.Domain/IntellaQuest.Domain.md`](IntellaQuest.Domain/IntellaQuest.Domain.md) |
| `IntellaQeust.Business` ⚠️ | `IntellaQeust.BusinessLogic/` | [`IntellaQeust.BusinessLogic/IntellaQeust.BusinessLogic.md`](IntellaQeust.BusinessLogic/IntellaQeust.BusinessLogic.md) |
| `IntellaQuest.Data.NHibernate` | `IntellaQuest.Repository/` | [`IntellaQuest.Repository/IntellaQuest.Repository.md`](IntellaQuest.Repository/IntellaQuest.Repository.md) |
| `IntellaQuest.Web` | `IntellaQuest.Web/` | [`IntellaQuest.Web/IntellaQuest.Web.md`](IntellaQuest.Web/IntellaQuest.Web.md) |
| `IntellaQuest.Migrations` | `IntellaQuest.Migrations/` | SQL migrations (one `.sql` file per entity) + a console runner that applies them all to `ShopDb` — see [`IntellaQuest.Migrations/README.md`](IntellaQuest.Migrations/README.md) |

⚠️ Note the typo: the BusinessLogic folder/project is **IntellaQeust** (Q-e-u-e-s-t), and namespaces are inconsistently `IntellaQeust.BusinessLogic.*` vs `IntellaQuest.BusinessLogic.*` — both appear in the code.

## Architecture

Layered, dependency direction: `Web → BusinessLogic → Repository → Domain`

- **Services** (`I*Service` + `*Service`, e.g. `IUserService`/`UserService`): business rules, transaction handling via `IUnitOfWork`, throw `BllException` with messages from `ShopExceptionMassages`.
- **Repositories** (`IRepository<T>` + `Repository<T>`): generic CRUD (`Add/Update/Delete/FindBy/All/FilterBy/CheckExist`) over NHibernate `ISession`. Per-entity repos add custom queries (e.g. `IUserRepository.GetUserRole`).
- **UnitOfWork**: wraps NHibernate session + transaction (`BeginTransaction`/`Commit`/`Rollback`).
- **Mappers**: extension methods (e.g. `user.MapToGridModel()`) converting entities ↔ view models.
- **DI:** Autofac, configured in `IntellaQuest.Web/Program.cs`. Convention-based registration: types ending in `Repository`/`Service` registered `AsImplementedInterfaces()`, all `InstancePerLifetimeScope`.
- **Validation:** FluentValidation. Validators are attached with `[FluentValidator(typeof(...))]` and executed by `Infrastructure/ValidationFilter.cs` into `ModelState`.

## Routing / API

Configured in `IntellaQuest.Web/Program.cs`:

1. API controllers use attribute routing (`[Route("api/[controller]/[action]/{id?}")]`). E.g. `POST /api/Identity/Login`, `POST /api/Products/...`
2. `{*path}` → `Home/Home` — catch-all serving the Angular SPA. A `SpaFallbackConstraint` keeps unknown `/api/...` URLs (and file-like paths) out of the SPA fallback so they return 404.

**API controllers** (`Controllers/Api/`, all extend `BaseController`, which converts unhandled exceptions to HTTP 409 + JSON `{success, exception}`):
`Identity` (auth, profile, payments), `Users`, `Products`, `Categories`, `Orders`, `ShoppingCarts`, `FavouriteProducts`, `ApplicationSettings`.

**Request binding:** `Infrastructure/JsonBodyModelBinder.cs` reproduces the MVC 5 JSON value-provider behaviour (route → query → matching JSON body property → whole body), so the existing Angular request shapes keep working unchanged.

## Auth & Security

- **JWT:** HMAC-SHA256, issued in `UserService.CreateToken`, 10-day expiry, claims: name / nameidentifier / role. Key, issuer, audience hardcoded in `TokenConstant.cs` ⚠️.
- **Passwords:** transformed via `PasswordEncryption.Encryption(...)` before storage/comparison (see `Security/PasswordEncryption.cs`).
- `MyJwtTokenCustomAuthorize` attribute exists in `IdentityController.cs` but is **incomplete** (`AuthorizeCore` always returns true; usage commented out). Endpoints are effectively unauthenticated ⚠️.

## Database

- SQL Server, connection string **hardcoded** in `NHibernateContext.cs`: `Data Source=localhost;Initial Catalog=ShopDb;Integrated Security=SSPI;TrustServerCertificate=True;` ⚠️
- Driver: `MicrosoftDataSqlClientDriver` (NHibernate) over `Microsoft.Data.SqlClient`; MS SQL 2005 dialect.
- Mappings: `IntellaQuest.Repository/Mapping/*Map.cs` (FluentNHibernate, auto-discovered from `CategoryMap` assembly).
- **SQL migrations:** `IntellaQuest.Migrations/` — migrations live in `<version>/<timestamp>/` folders (e.g. `Migrations/0.0.0.1/202601010000/`, the original per-entity `001_Role.sql`…`009_FavouriteProducts.sql` schema as version `0.0.0.1`), each with a `.cs` class plus `up.sql` (create) and `down.sql` (remove). A console runner creates `ShopDb` if needed and applies all pending migrations in order (tracked in `dbo.__SqlMigrations`). Run with `dotnet run --project IntellaQuest.Migrations`. See `IntellaQuest.Migrations/README.md`.
- ⚠️ `UnitOfWork` ctor calls `BuildSessionFactory()` each time — a new session factory per request/unit-of-work (inefficient; should be a singleton). NHibernate also opens a connection during this call (`SchemaMetadataUpdater`), so a reachable `ShopDb` is required.

## Frontend (Angular 20) — `IntellaQuest.Web/my-app/`

Run: `npm install` → `ng serve` (port 4200). Build: `npm run build` → outputs to `my-app/bundles/browser`, which `IntellaQuest.Web` serves as static files.

- **Auth:** `authentification/` (login, register) + `authentification.service.ts` (JWT via `@auth0/angular-jwt`)
- **Customer:** `home`, `category-products`, `product-components/` (grid + detail), `cart`, `order`/`order-components` (grid + detail), `favourite`, `profile`, `settings`, `side-filter`
- **Admin:** `admin-components/` — `admin-tool` shell + tools for users / products / orders (CRUD dialogs, datatables)
- **Routing:** `app-routing.module.ts` (e.g. `/profile/:id`, `/product-detail/:id`, `/admin-tool/users`, `**` → `/error-404`)

## Known Issues / Quirks

1. Typo'd namespace/folder `IntellaQeust.*` mixed with `IntellaQuest.*`
2. Hardcoded DB connection string and JWT signing key in source
3. Lots of commented-out code (old `FavouriteProducts` entity on `User`, login validation, dead methods in `UserService`)
4. `UserService.AddPayment` silently does nothing if `user.Payment != null` (only handles first payment)
5. JWT authorization not enforced (see Auth section)
6. Git: `.idea/` (Rider/ReSharper) is committed

## Commands

**Database (once):** `dotnet run --project IntellaQuest.Migrations` — creates `ShopDb` if needed and applies migrations.
**Backend:** `dotnet run --project IntellaQuest.Web` (already-built SPA served from `my-app/bundles/browser`).
**Frontend:** `cd IntellaQuest.Web/my-app && npm install && npm run build` (or `ng serve`).
