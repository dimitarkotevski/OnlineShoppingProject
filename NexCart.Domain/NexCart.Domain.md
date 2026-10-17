# NexCart.Domain — Project Doc

> Pure domain layer. **No dependencies** on other projects. Everything else builds on top of this.

## Structure

```
Domains/       Entity classes
Enums/         Enumerations
Properties/    AssemblyInfo.cs
```

## Entities (`Domains/`)

All entities extend `BaseEntity` (single `Guid Id` property) and use the NHibernate convention: **private `_camelCase` field + `virtual` property**.

| Entity | Key properties | Relationships |
|---|---|---|
| `User` | FirstName, LastName, Email, Username, Password, Description, Address, State, City, ZipCode, Amount (double), Role (`UserRole` enum) | Payment, Orders (`IList<Order>`), FavouriteProducts (`IList<Product>`) |
| `Product` | Name, Price (double), Image (`byte[]`), Description, Tags, Created (DateTime) | Category, Orders (`IList<Order>`) |
| `Category` | Name, Status (bool), Url | Products (`IList<Product>`) |
| `Order` | OrderName, TotalAmount (double), DateCreated, PaymentType, OrderStatus | User, ShoppingCart |
| `ShoppingCart` | Name, Active (bool) | User, ShoppingCartDetails (`IList<ShoppingCartDetail>`) |
| `ShoppingCartDetail` | Quantity (int) | ShoppingCart, Product |
| `Payment` | CardNumber, ExpirationDate, SecurityCode, CardHolder, City, BillingAdress ⚠️, ZipPostalCode, Country, PhoneNumber | — |
| `FavouriteProducts` | — | User, Product |

⚠️ `Payment.BillingAdress` is misspelled (should be `BillingAddress`) — renaming it requires a DB migration since FluentNHibernate maps it by name.

## Enums (`Enums/`)

| Enum | Values | Namespace ⚠️ |
|---|---|---|
| `OrderStatus` | Canceled=0, OnHold=1, PendingPayment=2, Completed=3 | `NexCart.Domain.Enums` |
| `PaymentType` | Cash=0, Card=1, NoPaymentTYpe=2 | `NexCart.Domain.Enum` (singular!) |
| `UserRole` | Admin=1, User=2 | `NexCart.Domain.Enums` |

⚠️ The three enums live in **different namespaces** (`Enums` vs `Enum` singular). `UserRole` is the source of truth for roles: stored as the `Users.Role` INT column (with a `CHECK` constraint) and written to the JWT role claim by name. Also `NoPaymentTYpe` is misspelled.

## Quirks

- `FavouriteProducts` entity exists but is **unused**: `User.FavouriteProducts` is `IList<Product>` (many-to-many via mapping), and the old code is commented out.
- No data annotations / validation attributes here — validation lives in BusinessLogic (FluentValidation).
