# NexCart — Online Shopping Web Site

A full-stack online shopping application: **ASP.NET Core (.NET 10) backend** with an **Angular 20 SPA** frontend.

## Technology Used

**Backend**
- .NET 10 / ASP.NET Core (MVC + attribute-routed API controllers)
- NHibernate 5.5 + FluentNHibernate (ORM) over SQL Server via `Microsoft.Data.SqlClient`
- Autofac (Dependency Injection)
- FluentValidation (model validation)
- JWT (HMAC-SHA256) for authentication

**Frontend** (`NexCart.Web/my-app/`)
- Angular 20 SPA, Angular Material, Bootstrap 5
- @auth0/angular-jwt for JWT handling (admin grids use plain HTML tables)

**Solution:** `NexCartProject.sln` (6 projects)

## Solution Structure

| Project | Folder | Responsibility |
|---|---|---|
| `NexCart.Domain` | `NexCart.Domain/` | Domain entities + enums. No dependencies. |
| `NexCart.Business` | `NexCart.Business/` | Services, view models, mappers, security, exceptions |
| `NexCart.Data.NHibernate` | `NexCart.Data.NHibernate/` | Generic `Repository<T>`, `IUnitOfWork`, `NHibernateContext`, mappings |
| `NexCart.Migrations` | `NexCart.Migrations/` | SQL migration scripts + console runner that applies them to `ShopDb` |
| `NexCart.Web` | `NexCart.Web/` | ASP.NET Core host, API controllers, Autofac wiring, Angular app |
| `NexCart.Tests` | `NexCart.Tests/` | xUnit tests for the business layer |

**Architecture:** layered, dependency direction `Web → BusinessLogic → Data.NHibernate → Domain`
- **Services** (`I*Service` + `*Service`): business rules, transactions via `IUnitOfWork`, throw `BllException`
- **Repositories** (`IRepository<T>` + `Repository<T>`): generic CRUD over NHibernate `ISession`
- **Mappers**: extension methods converting entities ↔ view models
- **DI:** Autofac, convention-based registration in `NexCart.Web/Program.cs` (`InstancePerLifetimeScope`)

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
- **Authentication:** `UserService.Login` verifies the username + password (PBKDF2 via `PasswordHasher`, migrating legacy AES values on first login), then issues a JWT (claims: name / nameidentifier / role) created in `CreateToken` from the configured `Jwt` settings. Endpoints are protected with `[Authorize]`, with an `AdminOnly` policy for admin operations.
- **Registration:** checks unique username/email, creates the user with the default customer role, then creates an empty active shopping cart.
- **Customer features:** browse categories/products, product detail, shopping cart, orders, favourite products, profile, payment methods, settings.
- **Admin features:** user / product / order management with filterable, sortable, paginated grids (`RequestModel` / `ResponseModel`).

**Frontend** (`NexCart.Web/my-app/`): `authentification/` (login/register), customer pages (`home`, `category-products`, `product-components`, `cart`, `order`, `favourite`, `profile`, `settings`), and `admin-components/` (admin-tool shell with CRUD dialogs). Routing in `app-routing.module.ts`.

## How to Run

**Database (once):** requires SQL Server reachable at `localhost` with a `ShopDb` database. Create it and apply the schema with the migrations runner:

```bash
dotnet run --project NexCart.Migrations
```

**Backend:**

```bash
dotnet run --project NexCart.Web
```

Serves the API and the already-built Angular bundle from `NexCart.Web/my-app/bundles/browser`
(override the URL with `--urls`, e.g. `dotnet run --project NexCart.Web --urls http://localhost:5080`).

**Frontend** (only needed when changing the Angular code):

```bash
cd NexCart.Web/my-app
npm install
npm run build    # outputs to bundles/browser, which NexCart.Web serves
# or: ng serve   # dev server on port 4200
```

**Tests:**

```bash
dotnet test NexCart.Tests
```

## Known Issues

1. `UserService.AddPayment` silently does nothing if the user already has a payment method
2. Lots of commented-out code
