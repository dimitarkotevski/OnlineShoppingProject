# NexCart shop frontend — `my-app`

Angular 20 single-page application for the customer-facing store. It is served by
**NexCart.Web** at <http://localhost:5080> and talks to that host's `/api` endpoints.

> Full documentation: **[Frontend docs](../../docs/frontend/README.md)** ·
> [Shop guide](../../docs/frontend/shop-app.md) ·
> [API integration](../../docs/frontend/api-integration.md) ·
> [Authentication](../../docs/frontend/authentication.md) ·
> [Build & deployment](../../docs/frontend/build-and-deploy.md) ·
> [Review & fixes](FRONTEND-FIXES.md)

## Quick start

The shop SPA is built automatically by `NexCart.Web.csproj` (`npm ci` on first build, then
`npm run build` into `bundles/browser`):

```powershell
# from the repository root
dotnet run --project NexCart.Web      # http://localhost:5080
```

Build the Angular app on its own:

```powershell
npm ci
npm run build       # -> bundles/browser (served by NexCart.Web)
npm run watch       # rebuild on change (development configuration)
npm start           # ng serve at http://localhost:4200 — UI only, no API proxy
npm test            # Karma/Jasmine (no specs currently)
```

`ng serve` does not forward `/api` to the backend. Use the ASP.NET host for end-to-end work,
or add a dev proxy as described in
[build & deployment](../../docs/frontend/build-and-deploy.md#development-server-and-api-calls).

## Stack

- Angular 20.3 (NgModules, strict templates), TypeScript 5.9, RxJS 7.8
- Angular Material 20 (dialogs/buttons), Bootstrap 5.3 + ng-bootstrap
- ngx-toastr notifications, Font Awesome icons
- `@auth0/angular-jwt` for decoding the signed JWT
- `@angular/build:application` builder, output to `bundles/browser`

## Source layout

```text
src/app/
├── authentification/       login, register, AuthentificationService
├── home/                   hero carousel + sorted product list
├── category-products/      category page, filters, CategoryService
├── customer-components/    shared product/cart/order UI, sort, filter, dialogs
├── cart/  order/           cart and active-order pages
├── favourite/              favourites page + service
├── profile/  settings/     account, payment and password screens
├── navbar/  footer/        shell navigation and footer
├── models/                 API/view models and enums
└── utils/                  interceptors, pipes, password policy, date helpers
```

Feature services call the API with relative URLs (`api/...`); there is no API URL environment
variable. Responses keep the backend's PascalCase fields and numeric enums.

## Routes

| URL | Page |
|---|---|
| `/` and `/sort/:sort` | Home (sort slugs: `popular`, `newest`, `best-selling`, `price-low`, `price-high`) |
| `/category/:category` | Category products |
| `/product-detail/:id` | Product detail |
| `/cart` | Shopping cart |
| `/my-order` | Active order |
| `/order-grid` | Order history |
| `/orders/:id/details` | Order detail route (not linked from the history grid yet) |
| `/favourite` | Favourite products |
| `/profile/:id` | Profile/payment (loads the signed-in user) |
| `/settings` | Account/address/password settings |
| `/login`, `/register` | Authentication |
| `/error-404`, `**` | Not found |

There are no Angular route guards; customer pages either check the session or rely on the
API's `401` response. Keep the shop and admin apps separate: administrators sign in at
<http://localhost:5090>, and the shop rejects administrator tokens.

## Notes

- The generated `bundles/` folder is gitignored; a fresh clone has no UI until the SPA is built.
- The authenticated identity (id, username, role) is decoded from the JWT; only `token` is
  stored in `localStorage`. See [Authentication](../../docs/frontend/authentication.md).
- The API uses PascalCase JSON, numeric enums, and `409 {success, exception}` /
  `400 {Success, Errors}` error envelopes. The global error interceptor turns failures into
  toasts; use `SKIP_ERROR_TOAST` when a page renders its own message.
- Known frontend defects and deferred items are recorded in [FRONTEND-FIXES.md](FRONTEND-FIXES.md).
