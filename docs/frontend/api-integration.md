# Frontend API integration

Both SPAs talk directly to the ASP.NET Core API of the host that serves them. There is no
generated client, no shared environment file with an API URL, and no `HttpClient` wrapper:
feature services call `HttpClient` and return typed Observables.

| Concern | Shop | Admin |
|---|---|---|
| API base | `api/...` | `/api/...` |
| Host | `NexCart.Web` (:5080) | `NexCart.Admin` (:5090) |
| Auth token | `localStorage.token` | `localStorage.adminToken` |
| Error toasts | `ErrorInterceptor` | `ErrorInterceptor` |
| Shared contract | PascalCase JSON, numeric enums, `POST` | same |

The backend configures `PropertyNamingPolicy = null`, so **response JSON keeps PascalCase**
(`TotalItems`, `ShoppingCartDetails`, `OrderName`, …). Frontend model fields mirror those
names exactly. Do not camel-case them.

## Request conventions

Almost every endpoint is a `POST` with an attribute route of the form
`api/[controller]/[action]/{id?}`. The custom `JsonBodyModelBinder` resolves action arguments
from, in order: the route, the query string, a matching JSON body property, then the whole
body. That means these shapes all work and are used in the app:

| Shape | Example | Where |
|---|---|---|
| Wrapped model | `{ "model": { ... } }` | registration, create/edit, set details, change password, payments, make-pay |
| Flat body fields | `{ "userId": "...", "productId": "..." }` | cart, favourites, order actions, payments |
| Id in the route | `POST api/Products/Get/{id}` with `null`/`JSON` body | product detail, user details, category lookups |
| Body id | `{ "Id": "..." }` or `{ "id": "..." }` | admin get/delete, user deletes |
| Id array | `{ "Ids": ["...", "..."] }` | admin bulk delete |
| Query string | `api/Products/GetAllBrands?categoryId=...` | brand filter |

The binder is case-insensitive for JSON properties. Follow the existing service when adding a
call rather than inventing a new envelope.

## Response conventions

- **Single item:** the view model itself (e.g. a `ProductViewModel` or `UserDetails`).
- **Shop lists:** `{ "TotalItems": number, "Items": [...] }` — `ResponseListModel<T>`.
- **Admin grids:** `{ "TotalItems", "Items", "Size", "CurrentPage" }` —
  `ResponseModel<T>`.
- **Booleans:** registration returns `true`/`false`; some admin actions return
  `{ success, deleted }` for bulk delete.
- **Enums:** numeric. `OrderStatus` = `0 Canceled`, `1 OnHold`, `2 PendingPayment`,
  `3 Completed`; `PaymentType` = `0 Cash`, `1 Card`; `UserRole` = `1 Admin`, `2 User`.
- **Dates:** ISO strings or legacy `/Date(ms)/`; convert with the `dateConverter` pipe.
- **Money:** the API sends plain numbers; the UI formats them (`currency` pipe or `$` prefix).

## Error handling

`BaseController` and `ApiExceptionMiddleware` return:

| Situation | Status | Body |
|---|---|---|
| Business/unhandled exception | `409 Conflict` | `{ "success": false, "exception": "message" }` |
| Validation failure | `400 Bad Request` | `{ "Success": false, "Errors": { "Field": ["message"] } }` |
| Missing/invalid/expired token | `401 Unauthorized` | — |
| Forbidden (not admin) | `403 Forbidden` | — |

`utils/error.interceptor.ts` (shop) and `core/error.interceptor.ts` (admin) turn any failed
response into a toastr error. They prefer `exception`, then joined `Errors`, then
`message`/`title`, then a status-based fallback. The error is always re-thrown so callers can
still react.

Use `SKIP_ERROR_TOAST` when the page shows its own message:

```ts
this.http.post(url, body, {
  context: new HttpContext().set(SKIP_ERROR_TOAST, true),
});
```

Current users of the skip token: shop login/registration, admin login, duplicate-favourite
handling. Do not suppress errors for ordinary grid loads — the interceptor is the only error
reporting for those pages.

## Service and endpoint matrix

### Shop (`NexCart.Web/my-app/src/app/`)

| Service | Endpoint | Method/action | Used by |
|---|---|---|---|
| `AuthentificationService` | `api/Identity/Login` | POST `{ Username, Password }` | Login |
| | `api/Identity/Registration` | POST `{ model: UserRegister }` | Register |
| | `api/Identity/GetUserDetails/{id}` | POST (route id) | Profile, settings |
| | `api/Identity/SetUserDetails` | POST `{ model: UserDetails }` | Settings |
| | `api/Identity/ChangePassword` | POST `{ model: ChangePasswordViewModel }` | Settings |
| | `api/Identity/GetAmountMoneyOfUser` | POST `{ userId }` | Navbar balance |
| | `api/Identity/AddPayment` | POST `{ userId, model }` | Profile |
| | `api/Identity/DeletePayment` | POST `{ userId }` | Profile |
| `ProductService` | `api/Products/All` | POST `RequestModel`/`ProductsFilter` | Home |
| | `api/Products/Get/{Id}` | POST (route id) | Product detail |
| | `api/Products/GetAllBrands?categoryId=` | POST (query) | Category filter |
| `CategoryService` | `api/Categories/AllCategories` | POST `null` | Navbar, category init |
| | `api/Categories/GetProductsByCategory/{Id}` | POST `RequestModel` | Category page |
| | `api/Categories/GetCategoryById/{Id}` | POST (route id) | Category header/breadcrumb |
| `OrderService` | `api/Orders/GetUserActiveOrder` | POST `{ userId }` | `/my-order` |
| | `api/Orders/GetUserNotActiveOrders` | POST `{ userId }` | Order history |
| | `api/Orders/CancelActiveOrder` | POST `{ orderId }` | Order detail |
| | `api/Orders/MakeAnOrder` | POST `{ shoppingCartId, userId }` | Cart checkout |
| | `api/Orders/MakeAPay` | POST `{ model: OrderViewModel }` | Payment confirmation |
| `ShoppingCartService` | `api/ShoppingCarts/GetUserShoppingCartWithProducts` | POST `{ userId }` | Cart |
| | `api/ShoppingCarts/AddProductToShoppingCart` | POST `{ userId, productId }` | Product grid/detail |
| | `api/ShoppingCarts/RemoveProductFromShoppingCart` | POST `{ detailsId }` | Cart |
| | `api/ShoppingCarts/MinusQuantity` / `PlusQuantity` | POST `{ detailsId }` | Cart |
| `FavouriteProductService` | `api/FavouriteProducts/GetUserFavouriteProducts` | POST `{ userId }` | Favourites |
| | `api/FavouriteProducts/UserAddFavouriteProduct` | POST `{ userId, productId }` | Product grid/detail |
| | `api/FavouriteProducts/RemoveFavouriteProduct` | POST `{ userId, productId }` | Favourites/cart grid |

The shop uses `RequestModel` (`models/request.ts`) for category requests:
`PriceFrom`, `PriceTo`, `SortName`, `SearchList` (brands), `SearchBrands`. Responses use
`ResponseListModel<T>`.

### Admin (`NexCart.Admin/admin-app/src/app/`)

| Service | Endpoint | Method/action | Used by |
|---|---|---|---|
| `AuthService` | `/api/Identity/AdminLogin` | POST `{ Username, Password }` | Login |
| `AdminToolProductService` | `/api/Products/All` | POST `RequestModel` | Products grid |
| | `/api/Products/Get` | POST `{ Id }` | Product lookup |
| | `/api/Products/Create` / `Edit` | POST `{ model }` | Product dialog |
| | `/api/Products/Delete` | POST `{ Id }` | Product delete |
| | `/api/Products/DeleteMany` | POST `{ Ids }` | Bulk delete |
| | `/api/Products/AddImageToProduct` | POST `{ id, image }` | Photo dialog |
| `AdminToolCategoryService` | `/api/Categories/All` | POST `RequestModel` | Categories grid |
| | `/api/Categories/Create` / `Edit` | POST `{ model }` | Category dialog |
| | `/api/Categories/Delete` | POST `{ Id }` | Category delete |
| `AdminToolOrderService` | `/api/Orders/All` | POST `RequestModel` | Orders grid |
| | `/api/Orders/Get` | POST `{ Id }` | Order lookup |
| | `/api/Orders/Create` / `Edit` | POST `{ model }` | Order dialog |
| | `/api/Orders/Delete` | POST `{ Id }` | Order delete |
| | `/api/Orders/DeleteMany` | POST `{ Ids }` | Bulk delete |
| `AdminToolUserService` | `/api/Users/All` | POST `RequestModel` | Users grid |
| | `/api/Users/Get` | POST `{ Id }` | User lookup |
| | `/api/Users/Create` / `Update` | POST `{ model }` | User dialog |
| | `/api/Users/Delete` | POST `{ Id }` | User delete |
| | `/api/Users/DeleteMany` | POST `{ Ids }` | Bulk delete |
| | `/api/Roles/All` | POST `{}` | Role select in user dialog |
| `CategoryService` | `/api/Categories/AllCategories` | POST `null` | Category lookup in dialogs |

Admin grid requests use `RequestModel` (`models/admin-tool-request-model.ts`); `getData()` calls
`apiRequest()` so empty filter values are removed before sending. `ResponseModel<T>` is the
grid envelope.

## Model notes

- Shop models live in `src/app/models/`; admin models in the admin app's `src/app/models/`.
  Both are thin classes mirroring the API fields the UI actually reads.
- `ProductGridModel` has `Id`, `Name`, `Price`, `Image`, `DateCreated`, `Category`.
- `OrderGridViewModel` has `Id`, `OrderName`, `OrderStatus`, `PaymentType`,
  `NumberOfProducts`, `TotalAmount`, `DateCreated`, and optional `User`/`ShoppingCart`
  lookups. The admin grid response does not populate those lookups, so the admin order
  dialog may show blank user/cart names.
- `ShoppingCart` in the shop is an empty placeholder; cart payloads are currently `any`.
- Models intentionally omit fields the UI does not use. Add to the interface/model when a new
  screen needs an extra field rather than relying on `any`.

## Adding an endpoint

1. Add the call to the existing feature service (or create a feature service).
2. Use a relative URL and the same `POST`/body style as the sibling actions.
3. Type the response where practical; otherwise keep the `any` local to the service method.
4. If the endpoint needs a success/error message, decide whether the global toast should stay
   (`SKIP_ERROR_TOAST` off) or the caller should render it.
5. If it returns a list/grid, map it to `ResponseListModel<T>` or `ResponseModel<T>` so the
   shared loading/empty states work.
