import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { RequestModel } from 'src/app/models/request';

@Component({ standalone: false,
  selector: 'app-side-filter',
  templateUrl: './side-filter.component.html',
  styleUrls: ['./side-filter.component.css']
})
export class SideFilterComponent implements OnChanges {

  @Output() refreshState: EventEmitter<RequestModel> = new EventEmitter<RequestModel>();
  @Input() brands?: string[] | null;
  /** Used to clear the visible selections when the parent switches category. */
  @Input() categoryId?: string;

  filterProducts: RequestModel = new RequestModel();
  filter: string[] = [];

  get hasActiveFilters(): boolean {
    return this.filter.length > 0
      || this.filterProducts.PriceFrom != null
      || this.filterProducts.PriceTo != null;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['categoryId'] && !changes['categoryId'].firstChange) {
      this.resetFilters();
    }
  }

  onChangeBrands(event: Event): void {
    const input = event.target as HTMLInputElement;
    const brand = input.value;

    if (input.checked) {
      if (!this.filter.includes(brand)) {
        this.filter.push(brand);
      }
    } else {
      this.filter = this.filter.filter(item => item !== brand);
    }

    // Keep the other filter values (price range, etc.); only the brand list changes.
    this.filterProducts.SearchBrands = this.filter;
    this.emitFilters();
  }

  searchPrice(): void {
    this.emitFilters();
  }

  clearFilters(): void {
    this.resetFilters();
    this.emitFilters();
  }

  private emitFilters(): void {
    // Emit a snapshot so the parent never holds a reference to the live form state.
    this.refreshState.emit({
      ...this.filterProducts,
      SearchBrands: [...this.filter]
    });
  }

  private resetFilters(): void {
    this.filter = [];
    this.filterProducts = new RequestModel();
  }
}
