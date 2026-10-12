# IntellaQuest — Online Shopping Web Site

A full-stack online shopping application: **.NET Framework backend** (ASP.NET MVC + Web API) with an **Angular SPA** frontend.

## Technology Used

**Backend**
- .NET Framework 4.7.2, ASP.NET MVC (`System.Web.Mvc`)
- NHibernate + FluentNHibernate (ORM)
- SQL Server (database)
- Autofac (Dependency Injection)
- FluentValidation (model validation)
- JWT (HMAC-SHA256) for authentication

**Frontend** (`IntellaQuest.Web/my-app/`)
- Angular 13 SPA, Angular Material, Bootstrap 5
- ngx-datatable (admin grids), @auth0/angular-jwt

**Solution:** `IntellaQuestProject.sln` (Visual Studio 2022, 4 projects)

## Solution Structure

| Project | Folder | Responsibility |
|---|---|---|
| `IntellaQuest.Domain` | `IntellaQuest.Domain/` | Domain entities + enums. No dependencies. |
| `IntellaQeust.BusinessLogic` ⚠️ | `IntellaQeust.BusinessLogic/` | Services, view models, mappers, security, exceptions |
| `IntellaQuest.Data.NHibernate` | `IntellaQuest.Repository/` | Generic `Repository<T>`, `IUnitOfWork`, `NHibernateContext`, mappings |
| `IntellaQuest.Web` | `IntellaQuest.Web/` | ASP.NET MVC host, API controllers, Autofac wiring, Angular app |

> ⚠️ The BusinessLogic project/folder is spelled **IntellaQeust** (Q-e-u-e-s-t); namespaces mix `IntellaQeust.*` and `IntellaQuest.*` in the code.

**Architecture:** layered, dependency direction `Web → BusinessLogic → Repository → Domain`
- **Services** (`I*Service` + `*Service`): business rules, transactions via `IUnitOfWork`, throw `BllException`
- **Repositories** (`IRepository<T>` + `Repository<T>`): generic CRUD over NHibernate `ISession`
- **Mappers**: extension methods converting entities ↔ view models
- **DI:** Autofac, convention-based registration in `Global.asax.cs` (`InstancePerRequest`)

## Domain Model

`User`, `Role`, `Product`, `Category`, `Order`, `ShoppingCart` (+ `ShoppingCartDetail`), `Payment`, `FavouriteProducts`; enums `OrderStatus`, `PaymentType`.

## How the App Works

**Routing** (`App_Start/RouteConfig.cs`):
1. `api/{controller}/{action}/{id}` — the API (e.g. `POST /api/Identity/Login`)
2. `{*pathInfo}` → `Home/Home` — catch-all serving the Angular SPA

**API controllers** (`Controllers/Api/`, all extend `BaseController`):
`Identity` (auth, profile, payments), `Users`, `Products`, `Categories`, `Orders`, `ShoppingCarts`, `FavouriteProducts`, `ApplicationSettings`.

- **Error handling:** unhandled exceptions → HTTP 409 + `{success, exception}`; validation errors → HTTP 400 + `{Success, Errors}`
- **Authentication:** `UserService.Login` verifies username + encrypted password, then issues a JWT (10-day expiry, claims: name / nameidentifier / role) created in `CreateToken`. Passwords are transformed via `PasswordEncryption.Encryption` before storage/comparison.
- **Registration:** checks unique username/email, creates the user with the default customer role, then creates an empty active shopping cart.
- **Customer features:** browse categories/products, product detail, shopping cart, orders, favourite products, profile, payment methods, settings.
- **Admin features:** user / product / order management with filterable, sortable, paginated grids (`RequestModel` / `ResponseModel`).

**Frontend** (`IntellaQuest.Web/my-app/`): `authentification/` (login/register), customer pages (`home`, `category-products`, `product-components`, `cart`, `order`, `favourite`, `profile`, `settings`), and `admin-components/` (admin-tool shell with CRUD dialogs). Routing in `app-routing.module.ts`.

## How to Run

**Backend:** open `IntellaQuestProject.sln` in Visual Studio 2022 → IIS Express. Requires SQL Server with a `ShopDb` database (connection string uses Integrated Security — see "Known Issues").

**Frontend:**
```bash
cd IntellaQuest.Web/my-app
npm install
ng serve    # runs on port 4200
```

## Known Issues

1. Typo'd namespace/folder `IntellaQeust.*` mixed with `IntellaQuest.*`
2. Hardcoded DB connection string (`NHibernateContext.cs`) and JWT signing key (`TokenConstant.cs`)
3. JWT authorization not enforced (`MyJwtTokenCustomAuthorize` is incomplete; endpoints are effectively unauthenticated)
4. `UnitOfWork` builds a new NHibernate session factory per unit-of-work (should be a singleton)
5. `UserService.AddPayment` silently does nothing if the user already has a payment method
6. Lots of commented-out code; frontend dep mismatch (`@angular/localize` v16 vs Angular 13)
7. Git: `.idea/` (Rider/ReSharper) is committed
