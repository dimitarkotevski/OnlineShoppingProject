# Admin app — `NexCart.Admin/admin-app`

Internal Angular 20 SPA served by **NexCart.Admin** at <http://localhost:5090>. It manages
products, categories, orders and users. The page is marked `noindex, nofollow` and is meant
for administrators only.

> This is a separate site from the shop. The shop's old `/admin` and `/admin-tool` links are
> redirected to this host, and `NexCart.Admin` serves its own admin-only API.

**Related:** [Frontend index](README.md) · [API integration](api-integration.md) ·
[Authentication](authentication.md) · [Build & deployment](build-and-deploy.md)

## Source map

```text
src/
├── app/
│   ├── app.component.ts           bare <router-outlet>
│   ├── app.module.ts              declarations, Material modules, interceptors
│   ├── app-routing.module.ts      /login + guarded admin layout
│   ├── core/
│   │   ├── auth.service.ts        admin login, token decoding, isAdmin/isAuthenticated
│   │   ├── auth.interceptor.ts    Bearer token for /api/*
│   │   ├── error.interceptor.ts   toastr + SKIP_ERROR_TOAST
│   │   ├── admin.guard.ts         CanActivate: admin session → layout
│   │   └── category.service.ts    category lookup for dialogs
│   ├── layout/                    sidebar/topbar shell around admin pages
│   ├── login/                     admin sign-in page
│   ├── products/
│   │   ├── admin-tool-products.component.*
│   │   ├── add-edit-product-dialog/
│   │   ├── photo-dialog/          image upload (max 5 MB, base64)
│   │   └── admin-tool-product-service/
│   ├── categories/
│   │   ├── admin-tool-categories.component.*
│   │   ├── add-edit-category/
│   │   └── admin-tool-category-service/
│   ├── orders/
│   │   ├── admin-tool-orders.component.*
│   │   ├── add-edit-order/
│   │   └── admin-tool-order-service/
│   ├── users/
│   │   ├── admin-tool-users.component.*
│   │   ├── add-edit-user/
│   │   └── admin-tool-user-service/
│   ├── models/                    grid/request/response models, enums
│   └── shared/
│       ├── filterable-grid.ts     shared search/filter/pagination base
│       ├── grid-selection.ts      current-page checkbox selection
│       ├── yes-no-dialog/         delete/cancel confirmation
│       ├── no-data/               empty state
│       └── pipes/                 date, order status, payment type, safe URL
├── assets/                        (empty in this app)
├── index.html                     <base href="/"> + noindex
├── main.ts                        bootstrap
├── styles.css, admin-tool.css     global/admin styles
└── custom-theme.scss              Angular Material theme
```

## Routes

Declared in `src/app/app-routing.module.ts`:

| URL | Component | Guard | Purpose |
|---|---|---|---|
| `/login` | `LoginComponent` | — | Admin sign-in; redirects to the requested page after login |
| `/` | `AdminLayoutComponent` | `adminGuard` | Shell with sidebar/topbar |
| `/products` | `AdminToolProductsComponent` | `adminGuard` | Default page (empty path redirects here) |
| `/categories` | `AdminToolCategoriesComponent` | `adminGuard` | Category management |
| `/orders` | `AdminToolOrdersComponent` | `adminGuard` | Order management |
| `/users` | `AdminToolUsersComponent` | `adminGuard` | User management |
| `**` | — | — | Redirects to `/products` |

`adminGuard` checks for a valid admin token; otherwise it redirects to
`/login?returnUrl=<attempted path>`. The login page accepts only in-app return paths (a path
starting with `/` but not `//`).

## Pages and features

| Page | Capabilities |
|---|---|
| **Login** | Username/password form, inline error, admin-only enforcement on the server (`Identity/AdminLogin`) and a second client check for the `admin` role |
| **Layout** | Sidebar links, current username, "Back to shop", logout; responsive menu on small screens |
| **Products** | Search, category/price/date/image filters, sorting (`Newest`, `PriceLow`, `PriceHigh`, `Popular`, `BestSelling`), page sizes 5–100, photo viewer/upload, add/edit, delete, bulk delete |
| **Categories** | Search/status filters, add/edit (name + active status), delete; deletion is blocked while a category has products; no bulk delete |
| **Orders** | Search, status/payment/price/date filters, page total amount, status edit, delete, bulk delete; completed orders are locked in the dialog. Orders are created by the shop — there is no "add order" button |
| **Users** | Search, role/city/email-ending filters, add/edit, delete, bulk delete; the signed-in admin's own row is excluded from bulk selection |

The product photo dialog accepts image files up to **5 MB**, converts them to base64 and calls
`Products/AddImageToProduct`. Photos are stored through the shared web root
(`NexCart.Web/wwwroot`) so the shop serves them too.

## Grid conventions

Most admin pages extend `shared/filterable-grid.ts` (`FilterableGrid`):

- `request: RequestModel` starts at `{ PageNeeded: 1, Size: 10 }`.
- `applyFilters()` validates the filter values client-side (mirroring the API validator), then
  resets to page 1 and calls the page's `getData()`.
- `apiRequest()` trims text and drops `null`/`undefined`/empty values, because the API rejects
  empty strings for numeric and date fields.
- `resetFilters()` clears the filter fields but keeps paging/search behaviour.
- Every grid shows a "Filters (n)" badge; the products grid additionally builds removable
  filter chips from the active fields.

`RequestModel` (`models/admin-tool-request-model.ts`) supports:

| Group | Fields |
|---|---|
| Paging/sorting | `PageNeeded`, `Size`, `SortName`, `isAscending` (string) |
| Search/text | `SearchString`, `City`, `EmailEnding` |
| Products/categories | `EntityId` (category), `PriceFrom`, `PriceTo`, `DateFrom`, `DateTo`, `HasImage`, `IsActive` |
| Orders | `OrderStatus`, `PaymentType`, `PriceFrom`, `PriceTo`, `DateFrom`, `DateTo` |
| Users | `Role` (1 = Admin, 2 = User) |

`ResponseModel<T>` is the grid envelope: `{ TotalItems, Items, Size, CurrentPage }`.

Behaviour shared by all grids:

- **Dialogs return data, the parent saves.** Add/edit dialogs close with a model; the page
  decides create vs. update by whether `Id` is present, then refreshes. Cancel/Esc/backdrop
  close with `undefined` and changes nothing.
- **Delete** always goes through `YesNoDialogComponent`.
- **Bulk delete** (`GridSelection`, used by products, orders and users) only covers the current
  page; selection is cleared on every reload, and pages step back one page when the last
  visible rows are removed.
- Pagination options are `5, 10, 20, 50, 100`.

## Authentication and session

- Sign-in posts to `Identity/AdminLogin`; the returned JWT is stored under **`adminToken`**
  (deliberately different from the shop's `token`).
- `AuthService` decodes username/id/role claims, checks expiry and requires the `admin` role.
  An unreadable or expired token is discarded.
- `AuthInterceptor` adds `Authorization: Bearer …` to requests whose URL starts with `/api/`;
  a `401` clears `adminToken`.
- The login page has its own error message, so the request uses `SKIP_ERROR_TOAST`.
- Administrators cannot use the shop: the shop rejects a token whose role is `admin`.
- There are no refresh tokens; expiry logs the admin out and the guard returns them to login.

See [Authentication](authentication.md) for the shared details and the differences between
the two apps.

## Build and serve

The admin app is **not** built by MSBuild. Build it into `admin-app/bundles/browser`, which
`NexCart.Admin` serves from the site root:

```powershell
cd NexCart.Admin/admin-app
npm ci
npm run build       # production build
# or: npm run watch # development rebuild on change
cd ../..
dotnet run --project NexCart.Admin
```

Open <http://localhost:5090>. There is no `npm start`/`ng serve` script; use the watch build
and refresh the host. The `bundles/` folder is gitignored, so a fresh clone needs a build
before the admin site has a UI.

## Extending the admin app

1. Create a feature folder under `src/app/<feature>/` with the grid page and a
   `<feature>-service` folder.
2. Declare the page (and any dialogs) in `AppModule`.
3. Add an `AdminTool<Feature>Service` that calls `/api/<Controller>/...` and returns typed
   Observables. Add grid/request models under `models/`.
4. Add the route as a child of the guarded layout in `app-routing.module.ts` and a nav item in
   `AdminLayoutComponent.navItems`.
5. For a filterable grid, extend `FilterableGrid`, implement `getData()`, and add any new
   filter fields to `RequestModel`, `FILTER_KEYS` and `validateFilters()`.
6. Use `GridSelection` for bulk actions and `YesNoDialogComponent` for destructive actions.
7. Rebuild the app and verify the page through the ASP.NET host.

## Known quirks

- The app has no test target or spec files; verify changes through the running host.
- The products page comment says default sorting is understood by the API's
  `ProductSortService`; keep the option values in sync with the backend.
- The admin app shares its ASP.NET host with no shop UI; the site root is the admin bundle,
  while product images come from `NexCart.Web/wwwroot`.
