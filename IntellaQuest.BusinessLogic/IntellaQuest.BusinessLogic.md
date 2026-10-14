# IntellaQuest.BusinessLogic — Project Doc

> Assembly: **IntellaQuest.BusinessLogic**. Business logic layer: services, view models, mappers, validation, security.

## Structure

```
Users/ Products/ Categories/ Orders/ Payments/
ShoppingCarts/ ShoppingCartDetails/ FavouriteProducts/
    <Aggregate>Models.cs      view models for the aggregate
    <Aggregate>Mappers.cs     entity ↔ view model extension methods
    <Aggregate>Service.cs     I<X>Service + X>Service
Security/          PasswordHasher, PasswordEncryption, JwtSettings
Exceptions/        BllException, ExceptionMassages/ShopExceptionMassages
RequestModel.cs, ResponseModel.cs, LookupViewModel.cs, FluentTableRequest.cs
```

## Services

All services follow the same pattern: constructor injection of `IUnitOfWork` + repositories; each method wraps work in `using (_unitOfWork.BeginTransaction())` … `_unitOfWork.Commit()`; errors thrown as `BllException` with messages from `ShopExceptionMassages`.

| Service | Key members |
|---|---|
| `IUserService` | `Registration`, `Login` → `UserTokenInformation` (JWT), `UpdatePassword`, `GetAll`, `Get`, `Create`, `Update`, `Delete`, `GetAmountMoneyOfUser`, `AddPayment`, `DeletePayment`, `MakeShoppingCardForTheUser` |
| `IProductService` | `GetAll`, `Get`, `Create`, `Update`, `Delete`, `GetProductImage`, `CheckNameExists`, `GetAllTable`, `UploadImage`, `GetAllBrands` |
| `ICategoryService` | `Get`, `GetAll`, `GetAll` (lookup list), `Create`, `Update`, `DeleteById`, `CheckCategoryNameExists`, `CheckCategoryStatus`, `FilterAndPage`, `GetProductsByCategory`, `GetCategoryById` |
| `IOrderService` | `GetAll`, `Get`, `Create`, `Update`, `Delete`, `ChangeOrderStatus`, `GetUserNotActiveOrders`, `GetUserActiveOrder`, `CancelActiveOrder`, `GetUserOtherOrders`, `MakeAnOrder`, `MakeAPay` |
| `IShoppingCartService` | `GetUserShoppingCartWithProducts`, `AddProductToShoppingCart`, `RemoveProductFromShoppingCart`, `PlusQuantity`, `MinusQuantity` |
| `IShoppingCartDetailService` | `AddShoppingCartDetailToShoppingCart`, `RemoveProductFromShoppingCart` |
| `IFavouriteProductService` | `AddProductToFavouriteProduct`, `RemoveProductFromFavuriteProduct` ⚠️ (typo in method name), `GetUserFavouriteProducts` |
| `IPaymentService` | **Empty interface** — stub, payment logic lives in `UserService` |

## Shared models

- **`RequestModel`** (`Requests` ns): `SearchString`, `SortName`, `isAscending`, `PageNeeded`, `Size`, `EntityId`, `SearchBrands`, `SearchList`, `PriceFrom`, `PriceTo`, `SearchStatus`, `EmailEnding` — used for grid filter/sort/paging.
- **`ResponseModel<T>`** (`Responses` ns): `TotalItems`, `Items`, `Size`, `CurrentPage` — paged result.
- **`ResponseListModel<T>`**: `TotalItems`, `Items` — non-paged list.
- `LookupViewModel`, `FluentTableRequest` (page/count).

## Security (`Security/`)

- **`PasswordHasher`** — PBKDF2-HMAC-SHA256 with a random per-password salt (stored as `PBKDF2$iterations$salt$hash`). Used for all new/changed passwords; `Verify` also accepts legacy values and `NeedsRehash` flags them for upgrade on login.
- **`PasswordEncryption`** — legacy reversible AES (Rfc2898DeriveBytes) with a hardcoded key; kept only so passwords stored by the old scheme can still be verified and migrated.
- **`JwtSettings`** — JWT settings bound from the `Jwt` configuration section: `Key` (required, at least 32 chars, never committed), `Issuer`, `Audience`, `ExpiryDays`. Tokens are HMAC-SHA256 (see `UserService.CreateToken`).

## Exceptions

- `BllException : Exception` — the only exception type thrown by services.
- `ShopExceptionMassages` — nested static classes per aggregate (`UserExceptionMassages`, `ProductsExceptionMassages`, `PaymentExceptionMassages`, `FavouriteProductExceptionMassages`, …) with `string.Format`-style constants.

## Quirks

- `UserService.Login` has its required-field validation **commented out**.
- `UserService.AddPayment` only handles the case where `user.Payment == null` — silently does nothing otherwise.
- Lots of commented-out dead code (e.g. `SetUserDetails`, old `UpdatePassword` at the bottom of `UserService.cs`).
