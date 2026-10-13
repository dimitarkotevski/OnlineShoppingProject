# IntellaQuest.Web — Project Doc

> Assembly: **IntellaQuest.Web**. ASP.NET Core (.NET 10) host — API controllers, Autofac DI, and the Angular SPA (`my-app/`).

## Startup (`Program.cs`)

Top-level `WebApplication` bootstrap:

1. **Autofac** via `builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())`; `ConfigureContainer` registers:
   - `UnitOfWork` as `IUnitOfWork` (`InstancePerLifetimeScope`)
   - **all types ending in `Repository` / `Service`** via `RegisterAssemblyTypes(...).AsImplementedInterfaces()`
   - FluentValidation `IValidator<>` implementations
2. **MVC:** `AddControllersWithViews`
   - `options.ModelBinderProviders.Insert(0, new JsonBodyModelBinderProvider())`
   - `options.Filters.Add<ValidationFilter>()`
   - `AddJsonOptions`: `PropertyNamingPolicy = null`, `DictionaryKeyPolicy = null`, `PropertyNameCaseInsensitive = true` (keeps the PascalCase JSON, and numeric enums, that the Angular client expects)
3. **`ApiBehaviorOptions`:** `SuppressModelStateInvalidFilter = true` and `SuppressInferBindingSourcesForParameters = true` — controllers validate `ModelState` themselves.
4. **CORS:** default policy allowing any origin/header/method.
5. **JWT bearer** authentication; `TokenValidationParameters` from `TokenConstant` (issuer/audience/key).
6. `SpaAssets` registered as a singleton.
7. Pipeline: `ApiExceptionMiddleware` → `UseCors` → static files (`my-app/bundles/browser`) → `UseStaticFiles` → `UseRouting` → auth → `MapControllers` → SPA catch-all.

## Routing

| Route | Purpose |
|---|---|
| `[Route("api/[controller]/[action]/{id?}")]` | Attribute routes on every API controller — e.g. `POST /api/Identity/Login` |
| `{*path}` → `Home/Home` | Catch-all: serves the Angular SPA shell, constrained by `SpaFallbackConstraint` |

`Infrastructure/SpaFallbackConstraint.cs` rejects API paths and file-like paths, so unknown `/api/...` URLs return **404** instead of the SPA shell.

## Infrastructure (`Infrastructure/`)

| File | Role |
|---|---|
| `JsonBodyModelBinder.cs` | `JsonBodyModelBinderProvider`/`JsonBodyModelBinder`: MVC 5-style body binding (route → query → matching JSON property → whole body) |
| `ValidationFilter.cs` | Global `IAsyncActionFilter` that runs `[FluentValidator]` validators into `ModelState` |
| `ApiExceptionMiddleware.cs` | Converts unhandled exceptions (including pre-action/database failures) on `/api/*` into HTTP 409 + `{success, exception}` |
| `SpaAssets.cs` | Resolves the hashed Angular filenames (`styles*`, `polyfills*`, `main*`) for the layout |
| `SpaFallbackConstraint.cs` | Route constraint excluding `/api` and files from the SPA fallback |

## Controllers

**`Controllers/HomeController.cs`** — `Home()` returns the SPA host view.
**`Controllers/ApplicationSettingsController.cs`** — `GetApplicationContext()`.

**`Controllers/Api/BaseController.cs`** — all API controllers extend it:
- `OnException` (`IExceptionFilter`, marked `[NonAction]`) → HTTP **409 Conflict** with JSON `{ success = false, exception = message }`
- `ErrorNotAcceptable()` (`[NonAction]`) → HTTP 400 with ModelState errors

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

- `Views/`: `Home/Home.cshtml` (SPA host), `Shared/_Layout.cshtml` (loads the hashed bundle via `SpaAssets`), `_ViewImports.cshtml`, `_ViewStart.cshtml`.
- `appsettings.json` / `appsettings.Development.json` — host configuration.
- `Properties/launchSettings.json` — `dotnet run` profile.

## Frontend — `my-app/` (Angular 20 SPA)

- Dev: `npm install` → `ng serve` at `http://localhost:4200`; build: `npm run build` → outputs to `my-app/bundles/browser`, which the host serves statically. (When using `ng serve`, the CORS default policy allows the dev origin.)
- Auth: `authentification/` (login, register) + `authentification.service.ts` (JWT via `@auth0/angular-jwt`).
- Customer: `home`, `category-products`, `product-components/` (grid + detail), `cart`, `order` + `order-components/` (grid + detail), `favourite`, `profile`, `settings`, `side-filter`.
- Admin: `admin-components/` — `admin-tool` shell + tools for **users / products / orders** (CRUD dialogs, ngx-datatable grids).
- Routing: `app-routing.module.ts` — `/`, `/sort/:sort`, `/category/:category`, `/register`, `/login`, `/profile/:id`, `/cart`, `/favourite`, `/settings`, `/product-detail/:id`, `/my-order`, `/order-grid`, `/orders/:id/details`, `/admin-tool[/products|/orders|/users]`, `**` → `/error-404`.

## Quirks

- **`MyJwtTokenCustomAuthorize`** (`IdentityController.cs`) is incomplete — `AuthorizeCore` always returns `true`, and its usages are commented out. **API endpoints are effectively unauthenticated.**
- CORS is currently wide open (any origin); restrict it before deployment.
- `.idea/` (JetBrains) is committed to git; consider adding to `.gitignore`.
