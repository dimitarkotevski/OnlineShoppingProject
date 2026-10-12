# IntellaQuest.Web — Project Doc

> Assembly: **IntellaQuest.Web**. ASP.NET MVC (System.Web.Mvc) host on .NET Framework 4.7.2 — API controllers, Autofac DI, and the Angular SPA (`my-app/`).

## Startup (`Global.asax.cs`)

`Application_Start`:
1. Autofac `ContainerBuilder`: registers controllers, `UnitOfWork` as `IUnitOfWork` (`InstancePerRequest`), and **all types ending in `Repository` / `Service`** via `RegisterAssemblyTypes(...).AsImplementedInterfaces()`.
2. `FluentValidationModelValidatorProvider.Configure()` + `RegisterModelBinderProvider()`.
3. `AreaRegistration`, `FilterConfig`, `RouteConfig`, `BundleConfig`.
4. `DependencyResolver.SetResolver(new AutofacDependencyResolver(container))`.

## Routing (`App_Start/RouteConfig.cs`)

| Route | Purpose |
|---|---|
| `api/{controller}/{action}/{id}` | The API — e.g. `POST /api/Identity/Login` |
| `{*pathInfo}` → `Home/Home` | Catch-all: serves the Angular SPA host page |

⚠️ No route constraint is actually applied (there's an unused `ServerRouteConstraint` in `App_Start/`), so the API prefix is by convention only.

## Controllers

**`Controllers/HomeController.cs`** — `Home()` returns the SPA host view.
**`Controllers/ApplicationSettingsController.cs`** — `GetApplicationContext()`.

**`Controllers/Api/BaseController.cs`** — all API controllers extend it:
- `OnException` → HTTP **409 Conflict** with JSON `{ success = false, exception = message }`
- `ErrorNotAcceptable()` → HTTP 400 with ModelState errors

| Controller | Actions |
|---|---|
| `IdentityController` | `Registration`, `Login`, `ChangePassword`, `GetUserDetails`, `SetUserDetails`, `UpdatePassword`, `GetAmountMoneyOfUser`, `AddPayment`, `DeletePayment` (+ incomplete `MyJwtTokenCustomAuthorize` attribute — see below) |
| `UsersController` | `All`, `Get`, `Create`, `Update`, `Delete` |
| `ProductsController` | `All`, `GetTable`, `Get`, `GetProductImage`, `Create`, `Edit`, `Delete`, `AddImageToProduct`, `GetAllBrands` |
| `CategoriesController` | `AllCategories`, `GetProductsByCategory`, `GetCategoryByUrl`, `All`, `Get`, `Create`, `Edit`, `DeleteById` |
| `OrdersController` | `All`, `Get`, `Create`, `Edit`, `Delete`, `GetUserActiveOrder`, `GetUserNotActiveOrders`, `CancelActiveOrder`, `MakeAnOrder`, `MakeAPay` |
| `ShoppingCartsController` | `GetUserShoppingCartWithProducts`, `AddProductToShoppingCart`, `RemoveProductFromShoppingCart`, `MinusQuantity`, `PlusQuantity` |
| `FavouriteProductsController` | `UserAddFavouriteProduct`, `GetUserFavouriteProducts`, `RemoveFavouriteProduct` |

## Other pieces

- `Cors/AllowCrossSiteAttribute.cs` — `ActionFilterAttribute` adding CORS headers for `http://localhost:4200` (Angular dev server). ⚠️ Check whether it's applied anywhere.
- `App_Start/`: `BundleConfig`, `FilterConfig`, `FluentValidationConfig` (body commented out — configured in `Global.asax` instead), `ServerRouteConstraint` (unused).
- `Views/`: `Home/Home.cshtml` (SPA host), `Shared/_Layout.cshtml`, `Shared/Error.cshtml`.

## Frontend — `my-app/` (Angular 13 SPA)

- Dev: `npm install` → `ng serve` at `http://localhost:4200`; build: `ng build`.
- Auth: `authentification/` (login, register) + `authentification.service.ts` (JWT via `@auth0/angular-jwt`).
- Customer: `home`, `category-products`, `product-components/` (grid + detail), `cart`, `order` + `order-components/` (grid + detail), `favourite`, `profile`, `settings`, `side-filter`.
- Admin: `admin-components/` — `admin-tool` shell + tools for **users / products / orders** (CRUD dialogs, ngx-datatable grids).
- Routing: `app-routing.module.ts` — `/`, `/sort/:sort`, `/category/:category`, `/register`, `/login`, `/profile/:id`, `/cart`, `/favourite`, `/settings`, `/product-detail/:id`, `/my-order`, `/order-grid`, `/orders/:id/details`, `/admin-tool[/products|/orders|/users]`, `**` → `/error-404`.

## Quirks

- **`MyJwtTokenCustomAuthorize`** (`IdentityController.cs`) is incomplete — `AuthorizeCore` always returns `true`, and its usages are commented out. **API endpoints are effectively unauthenticated.**
- CORS: `AllowCrossSiteAttribute` allows only `http://localhost:4200` — the deployed Angular app's origin must be added (or the attribute isn't used at all).
- `.idea/` (JetBrains) is committed to git; consider adding to `.gitignore`.
