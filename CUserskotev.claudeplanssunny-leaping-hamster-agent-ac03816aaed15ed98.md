# Filter Fix Plan

## Objectives
Fix the product filter functionality, addressing both UI/UX issues and missing backend logic.

## Frontend Investigation Findings
- **Component:** `SideFilterComponent`
- **State Management:** Uses `RequestModel` to track `PriceFrom`, `PriceTo`, `SearchBrands`, and `SearchList`.
- **Communication:** Emits `refreshState` event with the updated `RequestModel` whenever a filter changes.
- **UI Issues:**
    - The HTML structure uses Bootstrap cards but lacks proper accordion functionality (the `accordion` ID is present but no JS/Bootstrap data attributes for collapsing).
    - CSS file is empty, meaning it relies entirely on Bootstrap and potentially global styles.
    - Brand checkboxes are in a simple `<ul class="form-check">` with `<li>` items, which isn't the correct Bootstrap 5 structure (should be `div` with `form-check` class).
    - `onChange` method is defined in TS but not used in the HTML.

## Backend Investigation Findings
- **Request Model:** `RequestModel.cs` contains `SearchBrands`, `SearchList`, `PriceFrom`, and `PriceTo`.
- **Controller:** `ProductsController.All(RequestModel request)` calls `_productsService.GetAll(request)`.
- **Service:** `ProductService.FilterAndPage(RequestModel request)` **completely ignores** `SearchBrands`, `SearchList`, `PriceFrom`, and `PriceTo`. It only filters by `SearchString` and `EntityId`.
- **Note:** There is logic for these filters in `CategoryService.GetProductsByCategory`, but `ProductService.GetAll` (the main entry point for general product listing) doesn't use it.

## Proposed Changes

### 1. Frontend (UI/UX)
- Update `side-filter.component.html` to use correct Bootstrap 5 accordion and form-check structures.
- Implement basic styling in `side-filter.component.css` if needed to improve the "bad look".
- Ensure `SearchList` (if applicable) is integrated into the UI.

### 2. Backend (Logic)
- Implement filtering logic in `ProductService.FilterAndPage` to handle:
    - `PriceFrom` and `PriceTo` (range filtering).
    - `SearchBrands` (filter products by brand name).
    - `SearchList` (filter by description or tags).
- Ensure the logic follows the pattern seen in `CategoryService` but is optimized for the `Product` repository.

## Data Flow Trace
`SideFilterComponent` (UI Input) $\rightarrow$ `RequestModel` $\rightarrow$ `refreshState.emit()` $\rightarrow$ Parent Component $\rightarrow$ `ProductsController.All` $\rightarrow$ `ProductService.GetAll` $\rightarrow$ `ProductService.FilterAndPage` $\rightarrow$ NHibernate Query $\rightarrow$ Response.
