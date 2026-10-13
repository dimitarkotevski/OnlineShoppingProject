# IntellaQuest — Online Shopping Web Site

A full-stack online shopping application: **ASP.NET Core (.NET 10) backend** with an **Angular 20 SPA** frontend.

## Technology Used

**Backend**
- .NET 10 / ASP.NET Core (MVC + attribute-routed API controllers)
- NHibernate 5.5 + FluentNHibernate (ORM) over SQL Server via `Microsoft.Data.SqlClient`
- Autofac (Dependency Injection)
- FluentValidation (model validation)
- JWT (HMAC-SHA256) for authentication

**Frontend** (`IntellaQuest.Web/my-app/`)
- Angular 20 SPA, Angular Material, Bootstrap 5
- @auth0/angular-jwt for JWT handling (admin grids use plain HTML tables)

**Solution:** `IntellaQuestProject.sln` (5 projects)

## Solution Structure

| Project | Folder | Responsibility |
|---|---|---|
| `IntellaQuest.Domain` | `IntellaQuest.Domain/` | Domain entities + enums. No dependencies. |
| `IntellaQeust.BusinessLogic` ⚠️ | `IntellaQeust.BusinessLogic/` | Services, view models, mappers, security, exceptions |
| `IntellaQuest.Data.NHibernate` | `IntellaQuest.Repository/` | Generic `Repository<T>`, `IUnitOfWork`, `NHibernateContext`, mappings |
| `IntellaQuest.Migrations` | `IntellaQuest.Migrations/` | SQL migration scripts + console runner that applies them to `ShopDb` |
| `IntellaQuest.Web` | `IntellaQuest.Web/` | ASP.NET Core host, API controllers, Autofac wiring, Angular app |

> ⚠️ The BusinessLogic project/folder is spelled **IntellaQeust** (Q-e-u-e-s-t); namespaces mix `IntellaQeust.*` and `IntellaQuest.*` in the code.

**Architecture:** layered, dependency direction `Web → BusinessLogic → Repository → Domain`
- **Services** (`I*Service` + `*Service`): business rules, transactions via `IUnitOfWork`, throw `BllException`
- **Repositories** (`IRepository<T>` + `Repository<T>`): generic CRUD over NHibernate `ISession`
- **Mappers**: extension methods converting entities ↔ view models
- **DI:** Autofac, convention-based registration in `IntellaQuest.Web/Program.cs` (`InstancePerLifetimeScope`)

## Domain Model

`User`, `Role`, `Product`, `Category`, `Order`, `ShoppingCart` (+ `ShoppingCartDetail`), `Payment`, `FavouriteProducts`; enums `OrderStatus`, `PaymentType`.

## How the App Works

**Routing** (`Program.cs`):
1. API controllers are attribute-routed: `[Route("api/[controller]/[action]/{id?}")]` (e.g. `POST /api/Identity/Login`)
2. `{*path}` → `Home/Home` — catch-all serving the Angular SPA, constrained by `SpaFallbackConstraint` so unknown `/api/...` URLs return 404 instead of the shell

**API controllers** (`Controllers/Api/`, all extend `BaseController`):
`Identity` (auth, profile, payments), `Users`, `Products`, `Categories`, `Orders`, `ShoppingCarts`, `FavouriteProducts`, `ApplicationSettings`.

- **Request binding:** a custom `JsonBodyModelBinderProvider` reproduces the old MVC 5 behaviour — action values are resolved from the route, then the query string, then a matching JSON body property, and finally the whole body (so flat bodies, `{model: ...}` wrappers, and route ids all bind).
- **Error handling:** unhandled exceptions → HTTP 409 + `{success, exception}` (controller `IExceptionFilter`, plus `ApiExceptionMiddleware` for failures that happen before an action runs, e.g. database errors); validation errors → HTTP 400 + `{Success, Errors}`
- **Validation:** `ValidationFilter` runs the `[FluentValidator(typeof(...))]` validators into `ModelState`.
- **Authentication:** `UserService.Login` verifies username + encrypted password, then issues a JWT (10-day expiry, claims: name / nameidentifier / role) created in `CreateToken`. Passwords are transformed via `PasswordEncryption.Encryption` before storage/comparison.
- **Registration:** checks unique username/email, creates the user with the default customer role, then creates an empty active shopping cart.
- **Customer features:** browse categories/products, product detail, shopping cart, orders, favourite products, profile, payment methods, settings.
- **Admin features:** user / product / order management with filterable, sortable, paginated grids (`RequestModel` / `ResponseModel`).

**Frontend** (`IntellaQuest.Web/my-app/`): `authentification/` (login/register), customer pages (`home`, `category-products`, `product-components`, `cart`, `order`, `favourite`, `profile`, `settings`), and `admin-components/` (admin-tool shell with CRUD dialogs). Routing in `app-routing.module.ts`.

## How to Run

**Database (once):** requires SQL Server reachable at `localhost` with a `ShopDb` database. Create it and apply the schema with the migrations runner:

```bash
dotnet run --project IntellaQuest.Migrations
```

**Backend:**

```bash
dotnet run --project IntellaQuest.Web
```

Serves the API and the already-built Angular bundle from `IntellaQuest.Web/my-app/bundles/browser`
(override the URL with `--urls`, e.g. `dotnet run --project IntellaQuest.Web --urls http://localhost:5080`).

**Frontend** (only needed when changing the Angular code):

```bash
cd IntellaQuest.Web/my-app
npm install
npm run build    # outputs to bundles/browser, which IntellaQuest.Web serves
# or: ng serve   # dev server on port 4200
```

## Known Issues

1. Typo'd namespace/folder `IntellaQeust.*` mixed with `IntellaQuest.*`
2. Hardcoded DB connection string (`NHibernateContext.cs`) and JWT signing key (`TokenConstant.cs`)
3. JWT authorization not enforced (`MyJwtTokenCustomAuthorize` is incomplete; endpoints are effectively unauthenticated)
4. `UnitOfWork` builds a new NHibernate session factory per unit-of-work (should be a singleton)
5. `UserService.AddPayment` silently does nothing if the user already has a payment method
6. Lots of commented-out code
7. Git: `.idea/` (Rider/ReSharper) is committed
