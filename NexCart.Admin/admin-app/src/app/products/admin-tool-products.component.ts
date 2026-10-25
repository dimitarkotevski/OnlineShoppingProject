import { Component, OnInit } from '@angular/core';
import { AdminToolProductService } from './admin-tool-product-service/admin-tool-product.service';
import { ProductGridModel } from 'src/app/models/product-grid';
import { ResponseModel } from 'src/app/models/admin-tool-response-model';
import { RequestModel } from 'src/app/models/admin-tool-request-model';
import { MatDialog } from '@angular/material/dialog';
import { PhotoDialogComponent } from './photo-dialog/photo-dialog.component';
import { ProductViewModel } from 'src/app/models/product-view-model';
import { AddEditProductDialogComponent } from './add-edit-product-dialog/add-edit-product-dialog.component';
import { ToastrService } from 'ngx-toastr';
import { YesNoDialogComponent } from 'src/app/shared/yes-no-dialog/yes-no-dialog.component';
import { GridSelection } from 'src/app/shared/grid-selection';
import { FilterableGrid } from 'src/app/shared/filterable-grid';
import { CategoryService } from 'src/app/core/category.service';
import { CategoryViewModel } from 'src/app/models/category';

/** A removable chip describing one applied filter. */
interface FilterChip {
  key: keyof RequestModel;
  label: string;
}

@Component({ standalone: false,
  selector: 'app-admin-tool-products',
  templateUrl: './admin-tool-products.component.html',
  styleUrls: ['./admin-tool-products.component.css']
})
export class AdminToolProductsComponent extends FilterableGrid implements OnInit {

  response: ResponseModel<ProductGridModel>
  newItem: ProductViewModel;
  /** Rows ticked for bulk delete (current page only). */
  selection = new GridSelection();

  pagination: number[] = [5 , 10, 20, 50, 100]

  /** Options for the category filter. */
  categories: CategoryViewModel[] = [];

  /** True while a grid request is in flight (drives the table overlay). */
  loading = true;

  /** Search text of the last request, so the chips reflect what is actually applied. */
  private appliedSearch = '';

  /** Sort orders understood by the API's ProductSortService. */
  readonly sortOptions: { value: string | null; label: string }[] = [
    { value: null, label: 'Default order' },
    { value: 'Newest', label: 'Newest first' },
    { value: 'PriceLow', label: 'Price: low to high' },
    { value: 'PriceHigh', label: 'Price: high to low' },
    { value: 'Popular', label: 'Most favourited' },
    { value: 'BestSelling', label: 'Best selling' },
  ];

  constructor(
    private dialog: MatDialog,
    private toastr: ToastrService,
    private adminToolProductService: AdminToolProductService,
    private categoryService: CategoryService,
    ) {
      super();
      this.response = new ResponseModel<ProductGridModel>()
      this.newItem = new ProductViewModel;
    }

  ngOnInit(): void {

    this.request = {
      PageNeeded: 1,
      Size: 10,
    }

    this.categoryService.getAllCategories().subscribe((res: any) => {
      this.categories = res?.Items || [];
    });

    this.getData();
  }

  get totalPages(): number {
    return this.response.Size > 0 ? Math.ceil(this.response.TotalItems / this.response.Size) : 0;
  }

  /** First and last row numbers on the current page (for the "Showing x–y of z" line). */
  get showingFrom(): number {
    if (this.response.TotalItems === 0) {
      return 0;
    }
    return (this.response.CurrentPage - 1) * this.response.Size + 1;
  }

  get showingTo(): number {
    return Math.min(this.response.CurrentPage * this.response.Size, this.response.TotalItems);
  }

  /** True when a search or at least one filter is applied (changes the empty state). */
  get hasActiveSearchOrFilters(): boolean {
    return !!this.appliedSearch || this.activeFilters > 0;
  }

  /** Chips describing the applied search and filters; each one can be removed. */
  get filterChips(): FilterChip[] {
    const chips: FilterChip[] = [];
    const request = this.request;

    if (this.appliedSearch) {
      chips.push({ key: 'SearchString', label: `Search: “${this.appliedSearch}”` });
    }
    if (request.EntityId) {
      const category = this.categories.find(item => item.Id === request.EntityId);
      chips.push({ key: 'EntityId', label: `Category: ${category?.Name || 'selected'}` });
    }
    if (request.PriceFrom != null || request.PriceTo != null) {
      const from = request.PriceFrom != null ? `$${request.PriceFrom}` : 'any';
      const to = request.PriceTo != null ? `$${request.PriceTo}` : 'any';
      chips.push({ key: 'PriceFrom', label: `Price: ${from} – ${to}` });
    }
    if (request.DateFrom) {
      chips.push({ key: 'DateFrom', label: `Added from: ${request.DateFrom}` });
    }
    if (request.DateTo) {
      chips.push({ key: 'DateTo', label: `Added to: ${request.DateTo}` });
    }
    if (request.HasImage != null) {
      chips.push({ key: 'HasImage', label: request.HasImage ? 'With image' : 'Without image' });
    }

    return chips;
  }

  getData(){
    this.loading = true;
    this.appliedSearch = (this.request.SearchString || '').trim();

    this.adminToolProductService.getTable(this.apiRequest()).subscribe({
      next: (response: ResponseModel<ProductGridModel>) => {
        this.response = response;
        this.selection.clear();
        this.loading = false;
      },
      error: () => {
        // The failure is reported globally by the error interceptor.
        this.loading = false;
      }
    })
  }

  /** Re-queries from the first page when the sort order changes. */
  onSortChange(): void {
    this.request.PageNeeded = 1;
    this.getData();
  }

  /** Clears only the search box (the Reset button also clears every filter). */
  clearSearch(): void {
    this.request.SearchString = '';
    this.request.PageNeeded = 1;
    this.getData();
  }

  /** Removes a single applied filter (price is a paired range) and reloads. */
  clearFilter(key: keyof RequestModel): void {
    if (key === 'SearchString') {
      this.request.SearchString = '';
    } else if (key === 'PriceFrom' || key === 'PriceTo') {
      this.request.PriceFrom = null;
      this.request.PriceTo = null;
    } else {
      // null (not undefined) so select controls fall back to their "All" option.
      (this.request as any)[key] = null;
    }

    this.request.PageNeeded = 1;
    this.filterErrors = {};
    this.getData();
  }

  trackByProductId(_index: number, product: ProductGridModel): string {
    return product.Id;
  }

  trackByChip(_index: number, chip: FilterChip): string {
    return chip.key;
  }

  openPhotoModal(data64: string, id : string): void {
    const dialogRef = this.dialog.open(PhotoDialogComponent, {
      data: { data64, id},
      width: '600px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
  
    dialogRef.afterClosed().subscribe(result => {
      this.getData();
    });
  }
  addEditProduct(product?: ProductViewModel){
    const dialogRef = this.dialog.open(AddEditProductDialogComponent, {
      data: { product },
      width: '600px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
  
    dialogRef.afterClosed().subscribe((result : ProductViewModel) => {
      // Cancel / Esc / backdrop click close without a product: nothing to save.
      if(result){
        if(result.Id){
          this.adminToolProductService.update(result).subscribe(()=>{
            this.toastr.success("Success changed product")
            this.getData();
          })
        }else{
          this.adminToolProductService.create(result).subscribe(()=>{
            this.toastr.success("Success added product")
            this.getData();
          })
        }
      }
    });
    this.newItem = {
      Id: undefined,
      Name: "",
      Price: undefined,
      Description: "",
      Category: undefined
    };
  }

  deleteProduct(product?: ProductGridModel){
    const id = product?.Id;
    if(id){
      const name = product?.Name ? `“${product.Name}”` : 'this product';
      const dialogRef = this.dialog.open(YesNoDialogComponent, {
        data: { text: `Do you want to delete ${name}?` },
        width: '500px', // Set the width of the modal
        panelClass: ['custom-modalbox', 'admin-dialog']
      });
      dialogRef.afterClosed().subscribe((result : boolean) => {
        if(result){
          this.adminToolProductService.delete(id).subscribe(()=>{
            this.toastr.success("Success deleted product")
            this.getData()
          })
        }
      });
    }
  }

  /** Deletes every ticked row on the current page after a single confirmation. */
  deleteSelected(){
    const ids = this.selection.selectedIds;
    if(ids.length === 0){
      return;
    }
    const label = ids.length === 1 ? '1 product' : `${ids.length} products`;
    const dialogRef = this.dialog.open(YesNoDialogComponent, {
      data: { text: `Do you want to delete ${label}?` },
      width: '500px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if(confirmed){
        this.adminToolProductService.deleteMany(ids).subscribe(()=>{
          this.toastr.success(`Deleted ${label}`);
          // Step back a page when everything on the last page was removed.
          const page = this.request.PageNeeded || 1;
          if(ids.length >= (this.response.Items?.length || 0) && page > 1){
            this.request.PageNeeded = page - 1;
          }
          this.getData();
        })
      }
    });
  }

  onPageChange(value: number){
    this.request.PageNeeded = value;
    this.getData();
  }
  onPageSizeChange(value : number){
    this.request.Size = value;
    this.request.PageNeeded = 1;
    this.getData();
  }
}
