# Shop app — `NexCart.Web/my-app`

Customer-facing Angular 20 SPA, served by **NexCart.Web** at <http://localhost:5080>. It covers
product browsing, categories, cart, orders, favourites, profile/payment and account settings.

**Related:** [Frontend index](README.md) · [API integration](api-integration.md) ·
[Authentication](authentication.md) · [Build & deployment](build-and-deploy.md) ·
[Review & fixes](../../NexCart.Web/my-app/FRONTEND-FIXES.md)

## Source map

```text
src/
├── app/
│   ├── app.component.*            shell: navbar + router outlet + footer
│   ├── app.module.ts              all declarations, providers, interceptors
│   ├── app-routing.module.ts      top-level routes
│   ├── authentification/          login, register, AuthentificationService
│   ├── home/                      home page (carousel + sort bar + product grid)
│   ├── category-products/         category page + CategoryService
│   ├── customer-components/
│   │   ├── product-components/    product grid, product detail, ProductService
│   │   ├── cart-components/       cart grid + ShoppingCartService
│   │   ├── order-components/      order grid/detail/confirmation + OrderService
│   │   ├── side-filter/           brand/price/category filters
│   │   ├── sort-bar/              shared sorting bar (SORT_OPTIONS)
│   │   ├── breadcrumb/            page breadcrumb
│   │   ├── carousel/              home hero carousel
│   │   ├── loading-components/    grid skeleton and loading placeholder
│   │   ├── no-data/               reusable empty state
│   │   ├── yes-no-dialog/         confirmation dialog
│   │   └── error-page/            404 page
│   ├── cart/                      cart page shell
│   ├── order/                     active-order page
│   ├── favourite/                 favourites page + FavouriteProductService
│   ├── profile/                   profile, payment card, add-payment dialog
│   ├── settings/                  account / address / password tabs
│   ├── navbar/                    top navigation + category navbar
│   ├── footer/                    site footer
│   ├── models/                    API/view models and enums
│   ├── utils/                     interceptors, pipes, password policy, helpers
│   └── countries/                 static country list
├── assets/                        shop images
├── environments/                  production flag only (no API URL)
├── main.ts                        bootstrap + enableProdMode
└── styles.css, custom-theme.scss  global styles / Angular Material theme
```

The app uses **relative API URLs**, so it has no environment-specific backend address. The
ASP.NET host serves it from the same origin as the API.

## Routes

Routing is declared in `src/app/app-routing.module.ts`:

| URL | Component | Access | Notes |
|---|---|---|---|
| `/` | `HomeComponent` | Public | Product list, default sort `Popular` |
| `/sort/:sort` | `HomeComponent` | Public | Slugs: `popular`, `newest`, `best-selling`, `price-low`, `price-high` |
| `/category/:category` | `CategoryProductsComponent` | Public | `:category` is the category id |
| `/product-detail/:id` | `ProductDetailComponent` | Public | Add to cart/favourite requires a session |
| `/login` | `LoginComponent` | Guest | Redirects to `/` when already signed in |
| `/register` | `RegisterComponent` | Guest | Redirects to `/` when already signed in |
| `/cart` | `CartComponent` | Customer | Manual check → redirects to `/login` |
| `/my-order` | `OrderComponent` | Customer | Shows the active order, if any |
| `/order-grid` | `OrderGridComponent` | Customer | Read-only order history |
| `/orders/:id/details` | `OrderDetailComponent` | Customer | Route exists; no link from the history grid yet |
| `/favourite` | `FavouriteComponent` | Customer | Manual check → redirects to `/login` |
| `/profile/:id` | `ProfileComponent` | Customer | Loads the **signed-in** user; `:id` is not used |
| `/settings` | `SettingsComponent` | Customer | Profile, address and password tabs |
| `/error-404` | `ErrorPageComponent` | Public | Not-found page |
| `**` | — | Public | Redirects to `/error-404` |

> The shop has **no Angular route guards**. Pages that need a session either call
> `AuthentificationService.isAuthenticated()` / `getLoggedUserId()` and redirect, or rely on
> the API returning `401` (the auth interceptor then clears the token). Keep this in mind when
> adding a page: decide explicitly how it behaves for guests.

## Page and component catalogue

| Area | Components | Responsibility |
|---|---|---|
| Shell | `AppComponent`, `NavbarComponent`, `CategoriesNavbarComponent`, `RegularNavbarComponent`, `FooterComponent` | Auth-aware navigation, category menu, router outlet, page footer; logout refreshes shell state |
| Home | `HomeComponent`, `CarouselComponent`, `SortBarComponent`, `ProductsGridComponent` | Hero carousel and server-sorted product list |
| Category | `CategoryProductsComponent`, `BreadcrumbComponent`, `SideFilterComponent`, `SortBarComponent`, `ProductsGridComponent` | Category lookup, brand/price filtering, sorting, paged product grid |
| Product | `ProductDetailComponent`, `ProductsGridComponent` | Detail view; add to cart, buy now, add/remove favourite |
| Cart | `CartComponent`, `CartGridComponent` | Cart lines, quantity +/- (persisted via API), remove, checkout |
| Orders | `OrderComponent`, `OrderGridComponent`, `OrderDetailComponent`, `MakeOrderConfirmationComponent` | Active order, order history, cancellation, payment confirmation |
| Favourites | `FavouriteComponent`, `ProductsGridComponent` | Favourite list and removal |
| Profile | `ProfileComponent`, `AddPaymentComponent` | User details, masked payment card, add/delete payment |
| Settings | `SettingsComponent` | Profile/address updates, password change with client-side policy |
| Auth | `LoginComponent`, `RegisterComponent` | Credential forms, registration, flash message on redirect |
| Feedback | `LoadingGridComponent`, `SkeletonComponent`, `NoDataComponent`, `YesNoDialogComponent`, `ErrorPageComponent` | Loading, empty and confirmation states |

Pipes/utilities in `src/app/utils/`:

- `dateConverter` — handles legacy `/Date(ms)/` and ISO dates (1-based month).
- `orderStatus` / `paymentType` — render numeric API enums as labels.
- `byPassSecurity` — returns `SafeResourceUrl` for image bindings.
- `password-policy` — shared registration/change-password rules.
- `flash-message.service` — carries a toast across the full-page reload used after login/registration.
- `auth.interceptor` / `error.interceptor` — see [Authentication](authentication.md).

## Main flows

### Browse and sort

`HomeComponent` subscribes to `paramMap`; the `sort` segment maps to the API's `SortName`
(`Popular`, `Newest`, `BestSelling`, `PriceLow`, `PriceHigh`). The component loads products once
per navigation. The sort bar only navigates to `/sort/:slug`, so deep links work.

### Category filtering

`/category/:id` loads the category and its brands, then requests products for the category.
`SideFilterComponent` emits a `RequestModel` containing `SearchList`, `SearchBrands`,
`PriceFrom` and `PriceTo`; `CategoryProductsComponent` reapplies the current sort and reloads
the grid. Changing categories resets filters and sorting.

### Product actions

- **Add to cart** / **Buy now** call `ShoppingCarts/AddProductToShoppingCart`; buy-now then
  navigates to `/cart`.
- **Add to favourite** calls `FavouriteProducts/UserAddFavouriteProduct`; a duplicate error is
  shown as a friendly "already in favourites" warning.
- Guest cart/favourite actions redirect to `/login`; the product detail page returns before
  sending the call.

### Cart and checkout

`CartComponent` loads the current user's cart and renders it through
`ProductsGridComponent` in `type="cart"` mode as a responsive table (product, unit price,
quantity stepper, line subtotal, remove action and an order total footer). Quantity changes and
removals are persisted immediately (`MinusQuantity`, `PlusQuantity`,
`RemoveProductFromShoppingCart`). "Order here" opens `YesNoDialogComponent`, then calls
`Orders/MakeAnOrder` and navigates to `/my-order` with a full page reload.

### Orders and payment

- `/my-order` loads the active order (`GetUserActiveOrder`).
- "Make order" opens `MakeOrderConfirmationComponent`, chooses cash/card with numeric
  `[ngValue]`, and submits `Orders/MakeAPay`.
- "Cancel order" confirms, then calls `Orders/CancelActiveOrder`.
- `/order-grid` shows past orders from `GetUserNotActiveOrders`; it is read-only.

### Profile and settings

- `/profile/:id` loads the **signed-in** user's details (`GetUserDetails/{currentUserId}`).
  If there is no user id, it redirects to `/error-404`. The route id is currently ignored.
- Payment details are masked on the client; the card brand/expiry status is derived from the
  stored number and expiration date. Adding/deleting a payment uses `Identity/AddPayment` and
  `Identity/DeletePayment`, then reloads the page.
- `/settings` updates user details (`SetUserDetails`) and changes the password
  (`ChangePassword`); password rules are validated before sending.

### Authentication

Login and registration are covered in detail in [Authentication](authentication.md). Both use
a full-page reload after success (needed to reset all in-memory state) and queue a flash
message before navigating. The shop rejects administrator tokens: administrators must use
the [admin app](admin-app.md).

## Patterns and conventions

1. **NgModule architecture.** Components are declared in `AppModule`; use
   `standalone: false` explicitly when adding new components. Pipes are declared too.
2. **Services own HTTP.** Components subscribe to a feature service
   (`ProductService`, `CategoryService`, `OrderService`, `ShoppingCartService`,
   `FavouriteProductService`, `AuthentificationService`) rather than calling `HttpClient`
   directly. Services return Observables and use relative `api/...` URLs.
3. **Route-driven loading.** Prefer reading ids/sort from `ActivatedRoute` and loading once per
   navigation instead of keeping duplicate state and firing requests from multiple handlers.
4. **Reusable presentation components.** `ProductsGridComponent` is used by home, category,
   favourites, cart and order views through its `type` input. Parent pages listen to its
   `refreshState` output to reload data.
5. **Error handling is global.** The error interceptor turns API failures into toastr
   messages. Use `SKIP_ERROR_TOAST` for flows that render their own message (login,
   registration, duplicate favourites) so the user does not get two errors.
6. **Defensive templates.** Cart/favourite/order templates guard against `undefined` models and
   missing prices; keep that behaviour when editing them.
7. **Identity comes from the JWT.** Only `token` is stored; username, id and role are decoded
   from claims. Do not add separate plaintext identity keys to `localStorage`.
8. **Loading and empty states.** Use `LoadingGridComponent`/`SkeletonComponent` while a request
   is in flight and `NoDataComponent` for an empty result; do not render empty tables.

## Extending the shop

1. Create the component in the relevant feature folder with `standalone: false`, a selector,
   template and styles.
2. Declare it in `src/app/app.module.ts`.
3. Add a route in `app-routing.module.ts` (and a guard/redirect decision for guest access).
4. Add API calls to a feature service under the same folder; add typed models under
   `src/app/models/` using the backend's PascalCase field names.
5. Add navigation/links, then build with `npm run build` or let `dotnet run --project
   NexCart.Web` rebuild the SPA.
6. Update this guide if the route, flow or convention changes.

## Known quirks

- `/profile/:id` and `/orders/:id/details` accept ids in the URL, but neither component loads
  data from the route parameter. Profile always uses the signed-in user; the order-detail
  route is not linked from the order history table.
- `ShoppingCart` in `src/app/models/shopping-cart.ts` is an empty class; cart data is currently
  typed as `any` in the cart components.
- The home request still sends `Price`, while the API expects `PriceFrom`/`PriceTo`; the home
  page does not offer a price filter, so the value is ignored. The category side filter uses
  the correct fields.
- There are no `*.spec.ts` files in this app; `npm test` starts Karma but has no tests to run.
