# Frontend documentation

NexCart has **two Angular 20 SPAs**, each hosted by its own ASP.NET Core site. They share the
same UI stack and HTTP contract, but have separate sources, routes, build pipelines and login
sessions.

| App | Source | Served by | Local URL | JWT storage key |
|---|---|---|---|---|
| Customer shop | [`NexCart.Web/my-app/`](../../NexCart.Web/my-app/) | `NexCart.Web` | <http://localhost:5080> | `token` |
| Admin tool | [`NexCart.Admin/admin-app/`](../../NexCart.Admin/admin-app/) | `NexCart.Admin` | <http://localhost:5090> | `adminToken` |

Both apps are **NgModule-based** (not standalone), written in TypeScript 5.9 with strict
templates, and built with the `@angular/build:application` builder. UI building blocks:
Angular Material 20 (dialogs and admin form controls), Bootstrap 5 + ng-bootstrap,
ngx-toastr notifications, Font Awesome icons and `@auth0/angular-jwt` for token decoding.

## Documentation map

| Guide | What it covers |
|---|---|
| [Shop app](shop-app.md) | Customer SPA folder map, routes, pages, checkout flow, conventions |
| [Admin app](admin-app.md) | Admin SPA folder map, routes, grids, CRUD dialogs, bulk actions |
| [API integration](api-integration.md) | HTTP contract, service/endpoint matrix, models, error envelopes |
| [Authentication](authentication.md) | Shop and admin login flows, JWT storage, interceptors, guards |
| [Build & deployment](build-and-deploy.md) | npm scripts, bundles, hosting, CORS, tests, troubleshooting |
| [Frontend review & fixes](../../NexCart.Web/my-app/FRONTEND-FIXES.md) | Historical defects found in review and what was deferred |

Local entry points: [shop README](../../NexCart.Web/my-app/README.md) ·
[admin README](../../NexCart.Admin/admin-app/README.md) ·
[project overview](../../OVERVIEW.md) ·
[root README](../../README.md).

## Quick start

### Prerequisites

- **Node.js** supported by Angular 20 (`^20.19.0`, `^22.12.0`, or `>=24.0.0`) and npm.
- **.NET 10 SDK** for the two ASP.NET Core hosts.
- A reachable SQL Server `ShopDb` with migrations applied (see the
  [root README](../../README.md#how-to-run)).

### Shop

```powershell
dotnet run --project NexCart.Web
```

Open <http://localhost:5080>. `NexCart.Web.csproj` builds the Angular app automatically:
on the first build it runs `npm ci`, then `npm run build -- --configuration production`
into `my-app/bundles/browser`. Pass `-p:BuildSpa=false` to skip the SPA build.

To build the SPA on its own:

```powershell
cd NexCart.Web/my-app
npm ci
npm run build       # -> bundles/browser
# npm run watch     # rebuild on change (development config)
```

### Admin

```powershell
cd NexCart.Admin/admin-app
npm ci
npm run build       # or: npm run watch
cd ../..
dotnet run --project NexCart.Admin
```

Open <http://localhost:5090>. Unlike the shop, `NexCart.Admin.csproj` does **not** run npm;
the admin bundle must be built separately before the admin site can serve it.

The generated `bundles/` folders are gitignored in both apps. A fresh clone has no frontend
output until the corresponding build runs.

## Development server caveat

Both SPAs call the API with **relative URLs** (`api/...` in the shop, `/api/...` in the admin
app), and neither Angular project configures a dev proxy. Running `npm start` in the shop
serves the UI on <http://localhost:4200>, but API calls also go to port 4200 and will fail.
The admin app has no `start` script at all.

For end-to-end work, use the ASP.NET Core hosts above. If you want to use `ng serve`, add a
proxy file so `/api` is forwarded to the site:

```json
{
  "/api": {
    "target": "http://localhost:5080",
    "secure": false,
    "changeOrigin": true
  }
}
```

Then reference it from the `serve` target in `angular.json` (for the admin app, target
`http://localhost:5090`). The API CORS configuration already allows the dev origins
`http://localhost:4200` (shop) and `http://localhost:4300` (admin).

## Shared conventions

- Keep the backend's **PascalCase JSON** and **numeric enums** intact — the API uses
  `PropertyNamingPolicy = null` and the frontend models mirror the response field names.
- Every API call is a `POST`; put the payload in the body or in the route, following the
  existing service style (see [API integration](api-integration.md)).
- Failures are surfaced globally by `ErrorInterceptor` as toastr messages. Use
  `SKIP_ERROR_TOAST` when a page renders the failure inline.
- Store only the JWT in `localStorage`; user id, name and role are decoded from the token.
- Components live in feature folders under `src/app/`, use `standalone: false`, and must be
  declared in that app's `AppModule`.

## Verification

There are currently no `*.spec.ts` files. Karma/Jasmine is configured in the shop
(`npm test`); the admin app has no test target in `angular.json`. Build both apps and run the
manual smoke checks listed in
[troubleshooting](build-and-deploy.md#troubleshooting) before shipping a frontend change.
