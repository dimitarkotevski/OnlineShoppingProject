# NexCart admin frontend — `admin-app`

Angular 20 single-page application for the store administration tool. It is a **separate site**
from the shop: **NexCart.Admin** serves it at <http://localhost:5090> and exposes its own
admin-only API.

> Full documentation: **[Frontend docs](../../docs/frontend/README.md)** ·
> [Admin guide](../../docs/frontend/admin-app.md) ·
> [API integration](../../docs/frontend/api-integration.md) ·
> [Authentication](../../docs/frontend/authentication.md) ·
> [Build & deployment](../../docs/frontend/build-and-deploy.md)

## Quick start

Unlike the shop, this Angular app is **not built by MSBuild**. Build it first, then start the
ASP.NET host:

```powershell
npm ci
npm run build        # -> bundles/browser, served by NexCart.Admin
# or: npm run watch  # development rebuild; refresh the host to see changes

cd ../..
dotnet run --project NexCart.Admin      # http://localhost:5090
```

Open <http://localhost:5090> and sign in with an administrator account. The bundle folder is
gitignored, so a fresh clone needs the build above. There is no `npm start`/`ng serve` script.

## Stack

- Angular 20.3 (NgModules, strict templates), TypeScript 5.9, RxJS 7.8
- Angular Material 20 (dialogs, form fields, select, tooltip), Bootstrap 5 + ng-bootstrap
- ngx-toastr notifications, Font Awesome icons
- `@auth0/angular-jwt` for decoding the admin JWT
- `@angular/build:application` builder, output to `bundles/browser`

## Source layout

```text
src/app/
├── core/          AuthService, auth/error interceptors, admin guard, category lookup
├── layout/        sidebar + topbar shell
├── login/         admin sign-in
├── products/      grid, add/edit dialog, photo upload dialog, service
├── categories/    grid, add/edit dialog, service
├── orders/        grid, status dialog, service
├── users/         grid, add/edit dialog, service
├── models/        request/response/grid models and enums
└── shared/        FilterableGrid, GridSelection, dialogs, pipes
```

## Routes

| URL | Page | Access |
|---|---|---|
| `/login` | Admin sign-in | Public; redirects signed-in admins to `returnUrl`/`/products` |
| `/products` | Product management | `adminGuard` |
| `/categories` | Category management | `adminGuard` |
| `/orders` | Order management | `adminGuard` |
| `/users` | User management | `adminGuard` |
| `/` | Redirects to `/products` | `adminGuard` |

`adminGuard` requires a non-expired JWT with the `admin` role; otherwise it redirects to
`/login?returnUrl=...`. The admin session is stored under `adminToken` — deliberately separate
from the shop's `token`.

## How the grids work

Pages extend `shared/filterable-grid.ts`, which provides search/filter validation, filter
chips, paging state and `apiRequest()` (drops empty values before sending). The parent page
owns the API call and dialog result handling:

- add/edit dialogs return a model; presence of `Id` decides create vs. update;
- delete always goes through `YesNoDialogComponent`;
- `GridSelection` tracks ticked rows for bulk delete on the current page only;
- errors are reported once by the global error interceptor.

Request/response shapes and the full endpoint matrix are documented in
[API integration](../../docs/frontend/api-integration.md).

## Notes

- The admin host shares `NexCart.Web/wwwroot`, so uploaded product photos are served by the
  shop as well.
- The admin app has no test target or spec files; verify changes through the running host.
- There are no refresh tokens; an expired session returns the administrator to the login page.
- Product/category/order/user API calls all use the `/api/...` prefix so the auth interceptor
  attaches `Authorization: Bearer <adminToken>`.
