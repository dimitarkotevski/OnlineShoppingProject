# NexCart Online Shopping Project — Overview

> Context file for AI assistants. Full-stack online shopping web app.
> Last updated: 2026-10-14

## Overview

**NexCart** — an online shopping web site with an ASP.NET Core backend and an Angular SPA frontend.

- **Backend:** .NET 10 / ASP.NET Core (MVC + attribute-routed API controllers), NHibernate + FluentNHibernate (`Microsoft.Data.SqlClient` driver), SQL Server, Autofac DI, FluentValidation
- **Frontend:** Angular 20 SPA (lives inside `NexCart.Web/my-app/`), Angular Material, Bootstrap 5, @auth0/angular-jwt
- **Solution:** `NexCartProject.sln` (6 projects)

## Solution Structure

Each project also has its own detailed doc inside its folder:

| Project (assembly) | Folder | Doc |
|---|---|---|
| `NexCart.Domain` | `NexCart.Domain/` | [`NexCart.Domain/NexCart.Domain.md`](NexCart.Domain/NexCart.Domain.md) |
| `NexCart.Business` | `NexCart.Business/` | [`NexCart.Business/NexCart.Business.md`](NexCart.Business/NexCart.Business.md) |
| `NexCart.Data.NHibernate` | `NexCart.Data.NHibernate/` | [`NexCart.Data.NHibernate/NexCart.Data.NHibernate.md`](NexCart.Data.NHibernate/NexCart.Data.NHibernate.md) |
| `NexCart.Web` | `NexCart.Web/` | [`NexCart.Web/NexCart.Web.md`](NexCart.Web/NexCart.Web.md) |
| `NexCart.Migrations` | `NexCart.Migrations/` | FluentMigrator migrations + a console runner that applies them all to `ShopDb` — see [`NexCart.Migrations/README.md`](NexCart.Migrations/README.md) |
| `NexCart.Tests` | `NexCart.Tests/` | xUnit tests for the business layer (e.g. `PasswordHasher`) |

Every project follows the convention *folder = .csproj name = assembly name = root namespace*. MSBuild settings shared by all projects live in `Directory.Build.props`.

## Architecture

Layered, dependency direction: `Web → BusinessLogic → Data.NHibernate → Domain`

- **Services** (`I*Service` + `*Service`, e.g. `IUserService`/`UserService`): business rules, transaction handling via `IUnitOfWork`, throw `BllException` with messages from `ShopExceptionMassages`.
- **Repositories** (`IRepository<T>` + `Repository<T>`): generic CRUD (`Add/Update/Delete/FindBy/All/FilterBy/CheckExist`) over NHibernate `ISession`. Per-entity repos add custom queries (e.g. `IUserRepository.GetUserRole`).
- **UnitOfWork**: wraps NHibernate session + transaction (`BeginTransaction`/`Commit`/`Rollback`).
- **Mappers**: extension methods (e.g. `user.MapToGridModel()`) converting entities ↔ view models.
- **DI:** Autofac, configured in `NexCart.Web/Program.cs`. Convention-based registration: types ending in `Repository`/`Service` registered `AsImplementedInterfaces()`, all `InstancePerLifetimeScope`.
- **Validation:** FluentValidation. Validators are attached with `[FluentValidator(typeof(...))]` and executed by `Infrastructure/ValidationFilter.cs` into `ModelState`.

## Routing / API

Configured in `NexCart.Web/Program.cs`:

1. API controllers use attribute routing (`[Route("api/[controller]/[action]/{id?}")]`). E.g. `POST /api/Identity/Login`, `POST /api/Products/...`
2. `{*path}` → `Home/Home` — catch-all serving the Angular SPA. A `SpaFallbackConstraint` keeps unknown `/api/...` URLs (and file-like paths) out of the SPA fallback so they return 404.

**API controllers** (`Controllers/Api/`, all extend `BaseController`, which converts unhandled exceptions to HTTP 409 + JSON `{success, exception}`):
`Identity` (auth, profile, payments), `Users`, `Products`, `Categories`, `Orders`, `ShoppingCarts`, `FavouriteProducts`, `ApplicationSettings`.

**Request binding:** `Infrastructure/JsonBodyModelBinder.cs` reproduces the MVC 5 JSON value-provider behaviour (route → query → matching JSON body property → whole body), so the existing Angular request shapes keep working unchanged.

## Auth & Security

- **JWT:** HMAC-SHA256, issued in `UserService.CreateToken`, claims: name / nameidentifier / role. Key, issuer, audience and expiry are bound from the `Jwt` configuration section into `Security/JwtSettings.cs`; the signing key is required (at least 32 chars) and is never committed.
- **Passwords:** PBKDF2-HMAC-SHA256 with a random per-password salt (`Security/PasswordHasher.cs`). Passwords written by the old reversible-AES scheme (`PasswordEncryption`) are still verified, then re-hashed to PBKDF2 on the next successful login.
- **Authorization:** controllers/actions are protected with `[Authorize]` and an `AdminOnly` policy (configured in `Program.cs` using `ClaimsPrincipalExtensions.IsAdministrator`); security headers, HSTS/HTTPS redirection (outside Development), an authentication rate limiter and a restricted CORS policy are also configured in `Program.cs`.

## Database

- SQL Server; the connection string is read from configuration (`ConnectionStrings:ShopDb` in `NexCart.Web/appsettings.json`) and passed to `NHibernateContext.BuildSessionFactory`.
- Driver: `MicrosoftDataSqlClientDriver` (NHibernate) over `Microsoft.Data.SqlClient`; MS SQL 2005 dialect.
- Mappings: `NexCart.Data.NHibernate/Mapping/*Map.cs` (FluentNHibernate, auto-discovered from `CategoryMap` assembly).
- **SQL migrations:** `NexCart.Migrations/` — [FluentMigrator](https://fluentmigrator.github.io/) (`FluentMigrator`/`FluentMigrator.Runner`/`FluentMigrator.Runner.SqlServer` 8.0.1). Each migration lives in `Migrations/<version>/<timestamp>-<gitUser>/` as `sqlmigration.cs` plus `up.sql` / `down.sql` applied via `Execute.Script`. New ones are scaffolded with the `new-migration.tt` T4 template; the version lives in `NexCart.Migrations/appsettings.json` (`Migrations:Version`). A console runner creates `ShopDb` if needed and applies pending migrations in order; FluentMigrator tracks them in `dbo.VersionInfo`. Run with `dotnet run --project NexCart.Migrations` (`-- down` / `-- list`). See `NexCart.Migrations/README.md`.
- `UnitOfWork` takes an `ISessionFactoryProvider` (`ConfigurationRepository/SessionFactoryProvider.cs`) that builds the NHibernate session factory once and reuses it. Building it opens a connection, so a reachable `ShopDb` is required.

## Frontend (Angular 20) — `NexCart.Web/my-app/`

Run: `npm install` → `ng serve` (port 4200). Build: `npm run build` → outputs to `my-app/bundles/browser`, which `NexCart.Web` serves as static files.

- **Auth:** `authentification/` (login, register) + `authentification.service.ts` (JWT via `@auth0/angular-jwt`)
- **Customer:** `home`, `category-products`, `product-components/` (grid + detail), `cart`, `order`/`order-components` (grid + detail), `favourite`, `profile`, `settings`, `side-filter`
- **Admin:** `admin-components/` — `admin-tool` shell + tools for users / products / orders (CRUD dialogs, datatables)
- **Routing:** `app-routing.module.ts` (e.g. `/profile/:id`, `/product-detail/:id`, `/admin-tool/users`, `**` → `/error-404`)

## Known Issues / Quirks

1. Lots of commented-out code (old `FavouriteProducts` entity on `User`, login validation, dead methods in `UserService`)
2. `UserService.AddPayment` silently does nothing if `user.Payment != null` (only handles first payment)

## Commands

**Database (once):** `dotnet run --project NexCart.Migrations` — creates `ShopDb` if needed and applies migrations.
**Backend:** `dotnet run --project NexCart.Web` (already-built SPA served from `my-app/bundles/browser`).
**Frontend:** `cd NexCart.Web/my-app && npm install && npm run build` (or `ng serve`).
**Tests:** `dotnet test NexCart.Tests`.
