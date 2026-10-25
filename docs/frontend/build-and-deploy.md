# Frontend build and deployment

The two Angular apps have different build pipelines: **NexCart.Web builds its SPA from
MSBuild**, while **NexCart.Admin requires an explicit npm build**. The generated bundles are
gitignored in both apps.

## Prerequisites

- Node.js supported by Angular 20 (`^20.19.0`, `^22.12.0`, or `>=24.0.0`) and npm.
- .NET 10 SDK, SQL Server `ShopDb`, and the migrations applied once
  (`dotnet run --project NexCart.Migrations`).

## Commands

### Shop — `NexCart.Web/my-app`

| Command | What it does |
|---|---|
| `npm ci` | Install exactly the lockfile (the MSBuild target does this on first build) |
| `npm start` | `ng serve` on port 4200 — **UI only** (no API proxy; see below) |
| `npm run build` | Production build by default → `bundles/browser` |
| `npm run watch` | Development build/watch → `bundles/browser` |
| `npm run prod` | Explicit production build |
| `npm test` | Karma/Jasmine (no spec files currently) |

`dotnet build`/`dotnet run`/`dotnet publish` for `NexCart.Web` automatically:

1. runs `npm ci` when `my-app/node_modules` is missing;
2. runs `npm run build -- --configuration production` when any SPA source/config file is newer
   than `my-app/bundles/browser/index.html`;
3. copies the bundle next to the compiled assembly and into the publish output.

Useful properties:

```powershell
dotnet run --project NexCart.Web -p:BuildSpa=false          # skip npm entirely
dotnet build NexCart.Web -p:SpaBuildConfiguration=development
```

The shop host serves the bundle from two request paths: `/my-app/bundles/browser` (hashed
assets referenced by the Razor layout) and the site root. `SpaAssets` resolves the latest
`styles*`, `polyfills*`, `scripts*` and `main*` files. The SPA shell is `Views/Home/Home.cshtml`
rendered through `_Layout.cshtml`; client-side routes are handled by the `{*path}` catch-all.

### Admin — `NexCart.Admin/admin-app`

| Command | What it does |
|---|---|
| `npm ci` | Install exactly the lockfile |
| `npm run build` | Production build → `bundles/browser` |
| `npm run watch` | Development build/watch (refresh the host to see changes) |

There is no `serve`/`start` script and **no MSBuild integration**. Before `dotnet run
--project NexCart.Admin`, run one of the build commands above. The admin host serves
`admin-app/bundles/browser` from the site root and falls back to its `index.html` for Angular
routes; unknown `/api/...` URLs return 404.

### Full local startup

```powershell
# shop: ASP.NET host builds the SPA automatically
dotnet run --project NexCart.Web              # http://localhost:5080

# admin: build the SPA, then start the host
cd NexCart.Admin/admin-app
npm ci
npm run build
cd ../..
dotnet run --project NexCart.Admin            # http://localhost:5090
```

## Development server and API calls

The shop's `ng serve` (port 4200) does not proxy `/api`; the app uses relative `api/...` URLs,
so requests hit the dev server and fail. This is why the host-served app is the reliable local
workflow. If you want HMR with API access, create `proxy.conf.json`:

```json
{
  "/api": {
    "target": "http://localhost:5080",
    "secure": false,
    "changeOrigin": true
  }
}
```

and point the `serve` options at it (`"proxyConfig": "proxy.conf.json"`). For the admin app the
target is `http://localhost:5090`. The API CORS policy allows the dev origins
`http://localhost:4200` (shop) and `http://localhost:4300` (admin).

## CORS and hosting

| Host | Port | Allowed origins (`Cors:AllowedOrigins`) | Web content |
|---|---|---|---|
| `NexCart.Web` | 5080 | `http://localhost:4200`, `https://localhost:4200`, `http://localhost:5080`, `https://localhost:5080` | `my-app/bundles/browser` |
| `NexCart.Admin` | 5090 | `http://localhost:4300`, `https://localhost:4300`, `http://localhost:5090`, `https://localhost:5090` | `admin-app/bundles/browser` + shared `NexCart.Web/wwwroot` |

Only relative API calls are normal; CORS matters if you serve the Angular dev server or host
the API on a different origin. Add the exact origin to the relevant `appsettings.json` if you
change ports.

## Deployment checklist

1. **Shop:** `dotnet publish NexCart.Web -c Release` builds the SPA (unless `BuildSpa=false`)
   and includes `my-app/bundles/browser` in the publish output. Verify `index.html` exists in
   the published folder.
2. **Admin:** `npm ci && npm run build` in `NexCart.Admin/admin-app` first. The admin `.csproj`
   does not include the bundle in publish, so copy `admin-app/bundles/browser` into the
   deployed content root at the same relative path (for example
   `<publish>/admin-app/bundles/browser`).
3. Set the real `ConnectionStrings:ShopDb` and `Jwt:Key` (≥ 32 characters) in the deployed
   configuration; do not deploy development secrets.
4. Confirm the public URLs / HTTPS redirection and CORS origins match the deployed hosts.
5. Smoke-test both apps: shop browse → login → cart → order; admin login → each grid → one
   edit and one delete prompt.

Because Angular uses `outputHashing: "all"` in production, static filenames change per build;
serve `index.html`/the Razor shell and do not pin old bundle URLs.

## Troubleshooting

| Symptom | Likely cause / fix |
|---|---|
| `npm was not found` during `dotnet build` | Install Node/npm, or build with `-p:BuildSpa=false` |
| Shop page is blank / unstyled | No bundle or a stale build. Check `my-app/bundles/browser/index.html`; run `npm run build` or `dotnet build` without skipping the SPA |
| Admin shows 404/blank after clone | The admin bundle is not built or not copied. Run `npm ci && npm run build` in `admin-app`; for publish, copy the bundle manually |
| API calls fail under `ng serve` | No dev proxy; use the ASP.NET host or add `proxy.conf.json` |
| Browser shows old UI after a rebuild | Hard-refresh; hashed asset names prevent normal caching, but the shell may be cached |
| `401` and the user is suddenly signed out | Token expired/invalid; the interceptor clears it. Sign in again |
| Admin login says the account is not an administrator | `AdminLogin` only issues admin tokens; use an admin account. The client also checks the role |
| CORS error in the browser | Add the exact scheme/host/port to the host's `Cors:AllowedOrigins` |
| Backend refuses to start | Missing `ShopDb` connection or `Jwt:Key` too short outside Development |
| `dotnet run` rebuilds the SPA every time | Check file timestamps; the target is incremental on `index.html`. A stale/partial bundle folder can trigger rebuilds |
| Strict template/build errors after editing | Run `npm run build` directly for the full TypeScript/Angular diagnostic; fix rather than disabling strict flags |

## Manual smoke checks

Run these after a frontend change:

1. Shop home loads products for every sort tab and a deep link such as `/sort/newest`.
2. Category page: category, brands and price filter load; sorting and filter changes refetch.
3. Product detail: add to cart, buy now, add to favourites (guest → login redirect).
4. Cart: plus/minus/remove persist after reload; checkout creates the active order.
5. Orders: make a payment, cancel an order, history shows past orders.
6. Profile/settings: update details, add/delete payment, change password.
7. Admin: login, all four grids, filter/search/sort/page-size, add/edit/delete, bulk delete,
   product photo upload.
8. Both apps: expired/invalid token behavior after clearing `localStorage`.
