# IntellaQeust.BusinessLogic — Project Doc

> Assembly: **IntellaQeust.Business** (folder name has the typo — `IntellaQeust`, Q-e-u-e-s-t). Business logic layer: services, view models, mappers, validation, security.

## Structure

```
Users/ Products/ Categories/ Orders/ Payments/
ShoppingCarts/ ShoppingCartDetails/ FavouriteProducts/
    <Aggregate>Models.cs      view models for the aggregate
    <Aggregate>Mappers.cs     entity ↔ view model extension methods
    <Aggregate>Service.cs     I<X>Service + X>Service
Security/          PasswordEncryption, TokenConstant
Exceptions/        BllException, ExceptionMassages/ShopExceptionMassages
RequestModel.cs, ResponseModel.cs, LookupViewModel.cs, FluentTableRequest.cs
```

## Services

All services follow the same pattern: constructor injection of `IUnitOfWork` + repositories; each method wraps work in `using (_unitOfWork.BeginTransaction())` … `_unitOfWork.Commit()`; errors thrown as `BllException` with messages from `ShopExceptionMassages`.

| Service | Key members |
|---|---|
| `IUserService` | `Registration`, `Login` → `UserTokenInformation` (JWT), `UpdatePassword`, `GetAll`, `Get`, `Create`, `Update`, `Delete`, `GetAmountMoneyOfUser`, `AddPayment`, `DeletePayment`, `MakeShoppingCardForTheUser` |
| `IProductService` | `GetAll`, `Get`, `Create`, `Update`, `Delete`, `GetProductImage`, `CheckNameExists`, `GetAllTable`, `UploadImage`, `GetAllBrands` |
| `ICategoryService` | `Get`, `GetAll`, `GetAll` (lookup list), `Create`, `Update`, `DeleteById`, `CheckCategoryNameExists`, `CheckCategoryStatus`, `FilterAndPage`, `GetProductsByCategory`, `GetCategoryByUrl` |
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

- **`PasswordEncryption`** — AES (Rfc2898DeriveBytes) with **hardcoded** key `"MAKV2SPBNI99212"`; `Encryption()` / `Decryption()`. Reversible encryption, not hashing ⚠️.
- **`TokenConstant`** — JWT settings, all hardcoded: `KEY = "H2bXIQYGiLW5nADStMuVFPwg8hxRtSHi0ivvJbIfg"`, `ISSUER_CUSTOMERS = "CUSTOMER_ISSUE"`, `AUDIENCE_CUSTOMERS = "CUSTOMER_AUDIENCE"`. Tokens expire in **10 days** (see `UserService.CreateToken`).

## Exceptions

- `BllException : Exception` — the only exception type thrown by services.
- `ShopExceptionMassages` — nested static classes per aggregate (`UserExceptionMassages`, `ProductsExceptionMassages`, `PaymentExceptionMassages`, `FavouriteProductExceptionMassages`, …) with `string.Format`-style constants.

## Quirks

- Namespaces are inconsistent: `IntellaQeust.BusinessLogic.*` **and** `IntellaQuest.BusinessLogic.*` both appear (see `using` blocks in `UserService.cs`).
- `UserService.Login` has its required-field validation **commented out**.
- `UserService.AddPayment` only handles the case where `user.Payment == null` — silently does nothing otherwise.
- Lots of commented-out dead code (e.g. `SetUserDetails`, old `UpdatePassword` at the bottom of `UserService.cs`).
