# NexCart.Web — Project Doc

> Assembly: **NexCart.Web**. ASP.NET Core (.NET 10) host — API controllers, Autofac DI, and the Angular SPA (`my-app/`).

## Startup (`Program.cs`)

Top-level `WebApplication` bootstrap:

1. **Configuration / secrets** — the `ShopDb` connection string and the `Jwt` settings are read from configuration. A missing connection string (or a signing key shorter than 32 chars outside Development) fails fast; in Development an ephemeral key is generated when `Jwt:Key` is unset.
2. **Autofac** via `builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())`; `ConfigureContainer` registers:
   - `JwtSettings` (single instance)
   - `SessionFactoryProvider` as `ISessionFactoryProvider` (a **single instance**, so the NHibernate session factory is built once)
   - `UnitOfWork` as `IUnitOfWork` (`InstancePerLifetimeScope`)
   - **all types ending in `Repository` / `Service`** via `RegisterAssemblyTypes(...).AsImplementedInterfaces()`
   - FluentValidation `IValidator<>` implementations
3. **MVC:** `AddControllersWithViews`
   - `options.ModelBinderProviders.Insert(0, new JsonBodyModelBinderProvider())`
   - `options.Filters.Add<ValidationFilter>()`
   - `AddJsonOptions`: `PropertyNamingPolicy = null`, `DictionaryKeyPolicy = null`, `PropertyNameCaseInsensitive = true` (keeps the PascalCase JSON, and numeric enums, that the Angular client expects)
4. **`ApiBehaviorOptions`:** `SuppressModelStateInvalidFilter = true` and `SuppressInferBindingSourcesForParameters = true` — controllers validate `ModelState` themselves.
5. **CORS:** the default policy allows only the configured origins (`Cors:AllowedOrigins`, defaulting to the local dev ports).
6. **JWT bearer** authentication; `TokenValidationParameters` built from `JwtSettings` (issuer / audience / key), plus an `AdminOnly` authorization policy requiring `ClaimsPrincipalExtensions.IsAdministrator()`.
7. **Rate limiting:** a fixed-window `"auth"` policy (10 requests / minute per IP) throttles credential brute-forcing.
8. `SpaAssets` registered as a singleton.
9. Pipeline: `ApiExceptionMiddleware` → security headers → HSTS/HTTPS redirect (outside Development) → `UseCors` → static files (`my-app/bundles/browser`) → `UseStaticFiles` → `UseRouting` → `UseRateLimiter` → auth → `MapControllers` → SPA catch-all.

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
| `ClaimsPrincipalExtensions.cs` | `IsAdministrator()` — case-insensitive admin-role check used by the `AdminOnly` policy |
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
| `IdentityController` | `Registration`, `Login`, `ChangePassword`, `GetUserDetails`, `SetUserDetails`, `UpdatePassword`, `GetAmountMoneyOfUser`, `AddPayment`, `DeletePayment` (`Registration`/`Login` are `[AllowAnonymous]`; the rest are `[Authorize]`) |
| `UsersController` | `All`, `Get`, `Create`, `Update`, `Delete` (`AdminOnly` at controller level) |
| `ProductsController` | `All`, `GetTable`, `Get`, `GetProductImage`, `Create`, `Edit`, `Delete`, `AddImageToProduct`, `GetAllBrands` (writes are `AdminOnly`) |
| `CategoriesController` | `AllCategories`, `GetProductsByCategory`, `GetCategoryById`, `All`, `Get`, `Create`, `Edit`, `DeleteById` (writes are `AdminOnly`) |
| `OrdersController` | `All`, `Get`, `Create`, `Edit`, `Delete`, `GetUserActiveOrder`, `GetUserNotActiveOrders`, `CancelActiveOrder`, `MakeAnOrder`, `MakeAPay` (admin vs user actions are split) |
| `ShoppingCartsController` | `GetUserShoppingCartWithProducts`, `AddProductToShoppingCart`, `RemoveProductFromShoppingCart`, `MinusQuantity`, `PlusQuantity` (`[Authorize]` at controller level) |
| `FavouriteProductsController` | `UserAddFavouriteProduct`, `GetUserFavouriteProducts`, `RemoveFavouriteProduct` (`[Authorize]` at controller level) |

## Other pieces

- `Views/`: `Home/Home.cshtml` (SPA host), `Shared/_Layout.cshtml` (loads the hashed bundle via `SpaAssets`), `_ViewImports.cshtml`, `_ViewStart.cshtml`.
- `appsettings.json` / `appsettings.Development.json` — host configuration (`ConnectionStrings:ShopDb`, `Jwt`, `Cors:AllowedOrigins`).
- `Properties/launchSettings.json` — `dotnet run` profile.

## Frontend — `my-app/` (Angular 20 SPA)

Full guides: [frontend docs](../docs/frontend/README.md) · [shop app](../docs/frontend/shop-app.md) ·
[API integration](../docs/frontend/api-integration.md) · [authentication](../docs/frontend/authentication.md) ·
[build & deployment](../docs/frontend/build-and-deploy.md).

- **Build:** `NexCart.Web.csproj` builds the SPA automatically on `dotnet build`/`run`/`publish`
  (`npm ci` when needed, then `npm run build` into `my-app/bundles/browser`). Skip with
  `-p:BuildSpa=false`. The bundle is served through the Razor shell (`Views/Home/Home.cshtml`
  + `SpaAssets`), with a `{*path}` SPA fallback.
- **Dev UI only:** `npm start` (`ng serve`, port 4200). Relative `api/...` calls are not
  proxied, so use the ASP.NET host for API-backed development; add a `proxy.conf.json` if you
  need HMR against the API.
- **Auth:** `authentification/` (login/register) + `AuthentificationService`; JWT in
  `localStorage.token`, identity decoded from claims, admin tokens rejected. `AuthInterceptor`
  attaches the bearer token to `api/...`; `ErrorInterceptor` shows API errors as toasts.
- **Pages:** `home` (carousel + sorted grid), `category-products` (side filter + sort),
  `product-detail`, `cart`, `my-order` / `order-grid`, `favourite`, `profile` (payment),
  `settings` (account/address/password), `error-404`.
- **Routing:** `app-routing.module.ts` — routes listed in the [shop guide](../docs/frontend/shop-app.md#routes);
  there are no Angular route guards (pages check the session manually).
- **Admin:** the admin tool is a separate SPA/host (`NexCart.Admin/admin-app`) — see the
  [admin guide](../docs/frontend/admin-app.md). Old `/admin/...` links on this host redirect there.
- **Review & fixes:** see [my-app/FRONTEND-FIXES.md](my-app/FRONTEND-FIXES.md).

## Quirks

- In Development the JWT signing key is ephemeral when `Jwt:Key` is not configured, so tokens are invalidated on application restart.
- Outside Development the app requires an explicit `Jwt:Key` (at least 32 characters) and a `ShopDb` connection string.
