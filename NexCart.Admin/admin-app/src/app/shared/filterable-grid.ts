import {
  FILTER_KEYS,
  RequestModel,
  activeFilterCount,
  toApiRequest,
  validateFilters,
} from 'src/app/models/admin-tool-request-model';

/**
 * Search + filter behaviour shared by the admin grids. Subclasses load their rows in
 * `getData()` and should send `apiRequest()` rather than the raw request.
 */
export abstract class FilterableGrid {
  request: RequestModel = { PageNeeded: 1, Size: 10 };
  /** Validation message per filter field; empty when every filter is valid. */
  filterErrors: { [field: string]: string } = {};
  /** Whether the filter panel is expanded. */
  showFilters = false;

  abstract getData(): void;

  get activeFilters(): number {
    return activeFilterCount(this.request);
  }

  get hasFilterErrors(): boolean {
    return Object.keys(this.filterErrors).length > 0;
  }

  /** Re-validates as the user edits, so an error disappears once it is fixed. */
  revalidate(): void {
    if (this.hasFilterErrors) {
      this.filterErrors = validateFilters(this.request);
    }
  }

  /** Validates, then reloads from the first page (a narrower result may have fewer pages). */
  applyFilters(): void {
    this.filterErrors = validateFilters(this.request);
    if (this.hasFilterErrors) {
      this.showFilters = true;
      return;
    }
    this.request.PageNeeded = 1;
    this.getData();
  }

  /** Clears the search and every filter, keeping the page size. */
  resetFilters(): void {
    for (const key of FILTER_KEYS) {
      // null (not undefined) so select controls fall back to their "All" option.
      (this.request as any)[key] = null;
    }
    this.request.SearchString = '';
    this.filterErrors = {};
    this.request.PageNeeded = 1;
    this.getData();
  }

  protected apiRequest(): RequestModel {
    return toApiRequest(this.request);
  }
}
