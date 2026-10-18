import { Component, EventEmitter, Input, Output } from '@angular/core';

/** One entry of the storefront sorting bar; `value` is the SortName the API expects. */
export interface SortOption {
  label: string;
  value: string;
  /** Segment used in the home page `sort/:sort` route. */
  slug: string;
}

export const SORT_OPTIONS: SortOption[] = [
  { label: 'Popular', value: 'Popular', slug: 'popular' },
  { label: 'Newest', value: 'Newest', slug: 'newest' },
  { label: 'Best Selling', value: 'BestSelling', slug: 'best-selling' },
  { label: 'Price (low)', value: 'PriceLow', slug: 'price-low' },
  { label: 'Price (high)', value: 'PriceHigh', slug: 'price-high' },
];

/** Sorting bar shared by the home page and the category pages. Sorting itself is done by the API. */
@Component({
  standalone: false,
  selector: 'app-sort-bar',
  templateUrl: './sort-bar.component.html',
  styleUrls: ['./sort-bar.component.css']
})
export class SortBarComponent {
  readonly options = SORT_OPTIONS;

  /** SortName of the currently active option. */
  @Input() active: string = 'Popular';

  /** Emits the chosen option when the user picks a different sort. */
  @Output() sortChange = new EventEmitter<SortOption>();

  select(option: SortOption): void {
    if (option.value !== this.active) {
      this.sortChange.emit(option);
    }
  }
}
