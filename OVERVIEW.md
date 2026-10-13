# IntellaQuest Online Shopping Project — Overview

> Context file for AI assistants. Full-stack online shopping web app.
> Last updated: 2026-10-12

## Overview

**IntellaQuest** — an online shopping web site with a .NET backend and an Angular SPA frontend.

- **Backend:** .NET Framework 4.7.2, ASP.NET MVC (`System.Web.Mvc`), NHibernate + FluentNHibernate, SQL Server, Autofac DI, FluentValidation
- **Frontend:** Angular 13 SPA (lives inside `IntellaQuest.Web/my-app/`), Angular Material, Bootstrap 5, ngx-datatable, @auth0/angular-jwt
- **Solution:** `IntellaQuestProject.sln` (Visual Studio 2022, 5 projects)

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
- **DI:** Autofac, configured in `Global.asax.cs`. Convention-based registration: types ending in `Repository`/`Service` registered `AsImplementedInterfaces()`, all `InstancePerRequest`.
- **Validation:** FluentValidation, wired via `FluentValidationModelValidatorProvider.Configure()`.

## Routing / API

Route config (`App_Start/RouteConfig.cs`):

1. `api/{controller}/{action}/{id}` — the API. E.g. `POST /api/Identity/Login`, `POST /api/Products/...`
2. `{*pathInfo}` → `Home/Home` — catch-all serving the Angular SPA.

**API controllers** (`Controllers/Api/`, all extend `BaseController` which converts unhandled exceptions to HTTP 409 + JSON `{success, exception}`):
`Identity` (auth, profile, payments), `Users`, `Products`, `Categories`, `Orders`, `ShoppingCarts`, `FavouriteProducts`, `ApplicationSettings`.

## Auth & Security

- **JWT:** HMAC-SHA256, issued in `UserService.CreateToken`, 10-day expiry, claims: name / nameidentifier / role. Key, issuer, audience hardcoded in `TokenConstant.cs` ⚠️.
- **Passwords:** transformed via `PasswordEncryption.Encryption(...)` before storage/comparison (see `Security/PasswordEncryption.cs`).
- `MyJwtTokenCustomAuthorize` attribute exists in `IdentityController.cs` but is **incomplete** (`AuthorizeCore` always returns true; usage commented out). Endpoints are effectively unauthenticated ⚠️.

## Database

- SQL Server, connection string **hardcoded** in `NHibernateContext.cs`: `Data Source=localhost;Initial Catalog=ShopDb;Integrated Security=SSPI;` ⚠️
- Mappings: `IntellaQuest.Repository/Mapping/*Map.cs` (FluentNHibernate, auto-discovered from `CategoryMap` assembly).
- **SQL migrations:** `IntellaQuest.Migrations/` — one `.sql` file per entity (`Migrations/001_Role.sql` … `009_FavouriteProducts.sql`) plus a console runner that creates `ShopDb` if needed and applies all pending migrations in order (tracked in `dbo.__SqlMigrations`). Run it from VS or execute `IntellaQuest.Migrations.exe`. See `IntellaQuest.Migrations/README.md`.
- ⚠️ `UnitOfWork` ctor calls `BuildSessionFactory()` each time — a new session factory per request/unit-of-work (inefficient; should be a singleton).

## Frontend (Angular 13) — `IntellaQuest.Web/my-app/`

Run: `npm install` → `ng serve` (port 4200). Build: `ng build`.

- **Auth:** `authentification/` (login, register) + `authentification.service.ts` (JWT via `@auth0/angular-jwt`)
- **Customer:** `home`, `category-products`, `product-components/` (grid + detail), `cart`, `order`/`order-components` (grid + detail), `favourite`, `profile`, `settings`, `side-filter`
- **Admin:** `admin-components/` — `admin-tool` shell + tools for users / products / orders (CRUD dialogs, datatables)
- **Routing:** `app-routing.module.ts` (e.g. `/profile/:id`, `/product-detail/:id`, `/admin-tool/users`, `**` → `/error-404`)

## Known Issues / Quirks

1. Typo'd namespace/folder `IntellaQeust.*` mixed with `IntellaQuest.*`
2. Hardcoded DB connection string and JWT signing key in source
3. Lots of commented-out code (old `FavouriteProducts` entity on `User`, login validation, dead methods in `UserService`)
4. `UserService.AddPayment` silently does nothing if `user.Payment != null` (only handles first payment)
5. Frontend dep mismatch: `@angular/localize` v16 alongside Angular 13; `jsonwebtoken` in browser deps
6. JWT authorization not enforced (see Auth section)
7. Git: `.idea/` (Rider/ReSharper) is committed

## Commands

**Backend:** open `IntellaQuestProject.sln` in VS 2022 → IIS Express. Requires SQL Server with `ShopDb` database (Integrated Security).
**Frontend:** `cd IntellaQuest.Web/my-app && npm install && ng serve`
