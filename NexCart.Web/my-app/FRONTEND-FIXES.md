# Frontend (Angular) review & fixes

This document records the defects found while reviewing the Angular 20 storefront/admin
SPA in `NexCart.Web/my-app`, what was changed, and what was intentionally left
alone (with the reason). It is meant as a companion to `README.md` and
`NexCart.Web.md`.

All fixes preserve the existing HTTP contract with the ASP.NET Core API
(`POST api/{Controller}/{Action}`, PascalCase JSON, numeric enums).

---

## 1. Backend support change (needed by the frontend)

### `NexCart.Web/Infrastructure/JsonBodyModelBinder.cs`

- **Problem:** The binder looked up JSON body properties with a **case-sensitive**
  `JsonElement.TryGetProperty`. Several frontend calls post `{ "Id": ... }` while the
  action parameter is named `id` (`Users/Get`, `Users/Delete`, `Orders/Get`, ...).
  Those parameters silently bound to `Guid.Empty` because the property "Id" did not
  match the parameter name "id".
- **Fix:** Added `TryGetPropertyIgnoreCase`, which first tries an exact match and then
  falls back to an `OrdinalIgnoreCase` scan. This mirrors the case-insensitive matching
  of the old ASP.NET MVC 5 JSON value provider, so both `id` and `Id` bind correctly.
  Exact matches still win, so there is no ambiguity when a body contains both.

---

## 2. Core setup / build configuration

| File | Problem | Fix |
| --- | --- | --- |
| `src/environments/environment.ts` | `production: true` was hard-coded, so `ng serve` also ran in production mode. | Set `production: false`. |
| `angular.json` | The `production` build config never replaced `environment.ts` with `environment.prod.ts`. | Added a `fileReplacements` entry to the production configuration. Production builds now use `environment.prod.ts` (`production: true`); dev/server builds use `environment.ts` (`production: false`). |
| `angular.json` (test target) | Pointed at `src/test.ts` (`main`) and `karma.conf.js`. The `@angular/build:karma` builder no longer needs a `test.ts` entry point (it globs `**/*.spec.ts`), and `test.ts` uses `require.context`, which the esbuild pipeline cannot process. | Removed `main` and `karmaConfig` from the test options so the builder uses its built-in Karma config and spec globbing. |
| `karma.conf.js` | Required `@angular-devkit/build-angular` (framework + `plugins/karma`), but that package is not installed (Angular 20 uses `@angular/build`). Loading the file threw `MODULE_NOT_FOUND`. | Removed the `@angular-devkit/build-angular` framework/plugin entries; kept the Karma/Jasmine plugins that are installed. |
| `src/app/app.module.ts` | Dead `Routes` import and an empty `const routes`; a root-module `exports: [MatDialogModule]` that is meaningless; unused `NgxDatatableModule` import. | Removed all three. `ngx-datatable` was only referenced from a commented-out admin table. |
| `src/index.html` | `<body class=" mat-typography">` had a stray leading space. | Trimmed to `class="mat-typography"`. |
| `src/styles.css` | Invalid media query `@media (max-width: 575)` (missing unit) meant the block never applied. Duplicate `.solid_pb_3` and duplicate `html, body` rules. | Changed to `575px`; de-duplicated the `.solid_pb_3` and `html, body` rules. |
| `package.json` | `ngx-pagination` was declared but never imported. | Removed the dependency. |

---

## 3. Navbar event wiring

`NavbarComponent` / `RegularNavbarComponent` declared `refreshState` as an **`@Input`
holding an `EventEmitter`** and constructed it manually. The app shell listened to it as
`(refreshState)="refreshState()"`, which never fired because it was not an output.

- `src/app/navbar/navbar.component.ts` — `@Output() refreshState = new EventEmitter<any>()`;
  removed the constructor assignment, added the `Output` import.
- `src/app/navbar/regular-navbar/regular-navbar.component.ts` — same change.
- `src/app/navbar/navbar.component.html` — passes it on as an event:
  `(refreshState)="refreshState.emit()"` instead of `[refreshState]="refreshState"`.

Logging out now correctly refreshes the shell's authentication state.

---

## 4. Storefront (customer) fixes

| File | Problem | Fix |
| --- | --- | --- |
| `home/home.component.ts` + `.html` | Sorting links fired **two** requests (`ChangeSorting` fetched, then `ChangeRoute` navigated and re-created the component → `ngOnInit` fetched again). The `sort/:sort` route parameter was ignored, so deep links to a sort order did nothing. The sort callback read `products.recordsTotal`, which does not exist (the API returns `TotalItems`), so pagination totals were always `undefined`. | Rewrote the component to be route-driven: it subscribes to `paramMap`, maps the `sort` segment → `SortName`, and loads once per navigation. Removed `ChangeRoute`/duplicate fetch and read `TotalItems`. |
| `utils/convert-date.ts` | Used `getMonth()` (0-based) so every date was shown one month early; legacy `/Date(...)/` parsing via `substr` mis-handled ISO strings. | Added proper `/Date(ms)/` handling with an ISO fallback, `getMonth() + 1`, and an `isNaN` guard. |
| `utils/by-pass-security.ts` | Declared a `SafeResourceUrl` sanitizer (`bypassSecurityTrustResourceUrl`) but returned `SafeHtml`; the value is used in `[src]` bindings. | Returns `SafeResourceUrl`. |
| `cart/cart.component.html` | Empty-state block evaluated `shoppingCart.ShoppingCartDetails.length` while `shoppingCart` could still be `undefined` (template error / blank screen). | Guarded with `shoppingCart && ...`. |
| `cart-components/cart-grid/cart-grid.component.*` | `*ngFor`/empty-state touched `shoppingCart` before it was loaded; `detail.Product?.Price.toFixed(2)` threw when `Price` was unset; `handleMinus`/`handlePlus` changed the local quantity but never persisted it. | Guarded the template, used `Price?.toFixed(2)`, and call `MinusQuantity`/`PlusQuantity`. |
| `product-components/products-grid/products-grid.component.ts` | `addToCart` guarded with `!userId && !productId` (AND), so it only redirected to login when **both** were missing. | Changed to `!userId || !productId`. |
| `product-components/products-grid/products-grid.component.html` | Cart line total `detail.Product?.Price.toFixed(2) * detail.Quantity` threw on missing price and produced `NaN`. Order table used `product.Product.Price.toFixed(2)` unguarded. | Compute with `((price || 0) * (quantity || 0)).toFixed(2)` and guard the order price. |
| `order-components/order-detail/order-detail.component.html` | `activeOrder.TotalAmount.toFixed(2)` threw when `TotalAmount` was unset. | Guarded with `(activeOrder.TotalAmount || 0).toFixed(2)`. |
| `order-components/.../make-order-confirmation.component.html` | Order-status / payment-type `<option value="0">` bound to a **numeric** `ngModel`, so the select stored the string `"0"`/`"1"` and the API received a string enum. | Switched to `[ngValue]="0"` etc. |
| `product-components/product-detail/product-detail.component.*` | "Add to cart" had no handler at all; "Add to favourite" was a no-op that only set an unused message. | Implemented both: inject `ShoppingCartService`, `FavouriteProductService`, `AuthentificationService`, `ToastrService`; wire the Add-to-cart button; real add/redirect-to-login behaviour for favourites. |
| `category-products/category-products.component.ts` | `refreshState()` subscribed to `route.paramMap` on **every** side-filter change, leaking a new subscription each time and re-fetching the category/brands unnecessarily. | Subscribed to the route once in `ngOnInit`; `refreshState` now only reloads the products for the current category. |
| `customer-components/side-filter/side-filter.component.ts` | `onChangeBrands` replaced the whole `RequestModel` with a new one, wiping the selected price range on every brand toggle. | Only updates `SearchBrands`; other filter values are preserved. |
| `favourite/favourite.component.ts` | Implemented `OnChanges`/`ngOnChanges` even though the component has no inputs, so it never ran (dead code, potential duplicate fetch if an input were ever added). | Removed `OnChanges`/`ngOnChanges`. |

---

## 5. Admin fixes

| File | Problem | Fix |
| --- | --- | --- |
| `admin-tool/admin-tool.component.ts` | After `router.navigate(['error-404'])` for a non-admin, execution continued and still tried to navigate into the admin area. | Added `return;`. |
| `admin-tool-users/admin-tool-users.component.ts` | Same missing `return`; delete dialog opened with `data: { id }` but the dialog reads `data.text`, so the prompt was blank; no error handler on grid load. | Added `return;`, passed `{ text: 'Do you want to delete this user?' }`, and added an error toast. |
| `admin-tool-products/admin-tool-products.component.ts` | Missing `return`; blank delete prompt; no error handler. | Added `return;`, `{ text: 'Do you want to delete this product?' }`, error toast. |
| `admin-tool-orders/admin-tool-orders.component.ts` | Missing `return`; blank delete prompt; success toasts said "product" for orders; no error handler. | Added `return;`, `{ text: 'Do you want to delete this order?' }`, corrected the toast wording, error toast. |
| `add-edit-user/add-edit-user.component.html` | `[selectedIndex]="model.Role"` bound a **string** to the numeric `selectedIndex` DOM property. | Removed `[selectedIndex]`; `[(ngModel)]` already selects the right option. |
| `add-edit-product-dialog/add-edit-product-dialog.component.html` | Two `[selectedIndex]="model.Category"` bindings (object to numeric property) plus redundant `[value]`/`[ngValue]`. | Removed `[selectedIndex]` and the redundant `[value]` bindings. |
| `add-edit-order/add-edit-order.component.html` | `[selectedIndex]="model.OrderStatus"` (redundant) and `<option value="0">` with a numeric `ngModel`, causing string enum values. | Removed `[selectedIndex]`; switched options to `[ngValue]="0"`. |

---

## 6. HTTP contract / models

| File | Problem | Fix |
| --- | --- | --- |
| `authentification/authentification.service.ts` | `registerUser` mapped the response to `UserTokenInfo` and checked `Id`/`Token`/`Role`, but `Identity/Registration` returns a **boolean**. It always resolved to `false` while the caller ignored the value. | Returns `Observable<boolean>` matching the API. |
| `authentification/register/register.component.ts` | On success it always navigated, ignoring a `false` result; the error handler only read `error.error.exception` and missed the `{ Success, Errors }` validation envelope. | Handles `false`, and reads `Errors` (falling back to `exception`). |
| `models/product-view-model.ts` | Field was named `Created`, but the API serializes `DateCreated`. | Renamed to `DateCreated`. |
| `admin-tool-models/admin-tool-request-model.ts` | `isAscending?: boolean`, but the API `RequestModel.isAscending` is a `string`. | Changed to `string`. |
| `profile/profile.component.ts` | Successful "add payment" showed *"Success deleted payment"*. | Corrected to *"Success added payment"*. |
| `profile/add-payment/add-payment.component.ts` | `new Date(this.dateString).toISOString()` threw `RangeError: Invalid time value` when the month input was empty or invalid. | Guard against empty/invalid dates before calling `toISOString()`. |

---

## 7. Intentionally deferred (documented, not changed)

These were reviewed and are either harmless in practice or would require a product
decision / larger refactor, so they were left as-is:

- **`custom-theme.scss` uses `mat.all-component-themes`.** Deprecated in Angular
  Material 20 but still functional. Migrating to the `mat.theme()` API is a separate
  theming change.
- **Vendored `src/zone.umd.js` polyfill.** The build loads `./src/zone.umd.js`
  (tracked in git) instead of the `zone.js` package entry. It works; switching to
  `zone.js` should be validated against the runtime before changing.
- **`src/polyfills.ts`** is no longer referenced by `angular.json` (dead file). Left in
  place; it can be deleted once confirmed unused by tooling.
- **`src/test.ts`** still exists and uses `require.context`; it is no longer the test
  entry point (the Karma target globs spec files instead). There are currently no
  `*.spec.ts` files.
- **`admin-tool/admin-tool.component.html` `<router-outlet>`** is unused because the
  admin sub-pages (`admin-tool/products`, `.../orders`, `.../users`) are siblings of
  `admin-tool`, not children. Harmless; removing it is cosmetic.
- **`OrderGridViewModel.User` / `.ShoppingCart`** exist on the frontend model but the
  `Orders/All` grid payload does not populate them, so the admin order dialog shows
  blank user/cart names. Populating them is a backend change.
- **`ProductsFilter.Price`** is sent by the home component but the API `RequestModel`
  exposes `PriceFrom`/`PriceTo` instead; the value is ignored. The side filter already
  uses the correct names.
- **Model completeness:** the frontend `UserDetails`/`UserGridModel`/`PaymentInfoViewModel`
  only model the fields the UI uses. Extra API fields (e.g. payment billing address,
  user `Role` lookup) are not typed. Add them when a screen needs them.
- **`MatSelectModule`** is imported in `AppModule` but the app uses native `<select>`.
  Harmless; can be dropped later.
- **`@swimlane/ngx-datatable`** is still declared in `package.json` for the
  commented-out admin grid markup. The module import was removed from `AppModule`.

---

## Notes for verification

Per the project workflow, the assistant did **not** run `dotnet build`/`ng build`.
Recommended manual checks by the owner:

1. `ng build` (production) and `ng serve` — confirm the production file replacement and
   `enableProdMode()` now behave correctly.
2. Home sorting tabs — one request per click, `TotalItems` correct, deep link
   `/sort/newest` works.
3. Add-to-cart / add-to-favourite from the product grid and product detail page.
4. Admin: delete prompts show text; non-admin is redirected before any data request.
5. Login/registration round-trip.
