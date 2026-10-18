import { Component, OnInit } from '@angular/core';
import { ProductService } from '../customer-components/product-components/product.service';
import { ProductsFilter } from '../models/products-filter';
import { ActivatedRoute, Router } from '@angular/router';
import { ResponseListModel } from '../models/response';
import { ProductGridModel } from '../models/product-grid';
import { SORT_OPTIONS } from '../customer-components/sort-bar/sort-bar.component';


@Component({ standalone: false,
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  isUserAuthenticated: boolean = false;
  products: ProductGridModel[] | null | undefined;
  totalItems?: number;
  activeSorting: string = 'Popular';
  isLoading: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    ) { }

  ngOnInit(): void {
    // The route drives the sort order. Sorting happens on the server (Popular and
    // Best Selling need favourite and sales data), so each change reloads the products.
    this.route.paramMap.subscribe(params => {
      const sortUrl = params.get('sort') || 'popular';
      // Maps the `sort/:sort` route segment to the backend SortName value.
      this.activeSorting = SORT_OPTIONS.find(option => option.slug === sortUrl)?.value || 'Popular';
      this.loadProducts();
    });
  }

  private loadProducts(): void {
    const filter: ProductsFilter = {
      SearchString: "",
      SortName: this.activeSorting,
      PageNeeded: "",
      Price: "",
    };

    this.isLoading = true;
    this.productService.getAll(filter).subscribe({
      next: (result: ResponseListModel<ProductGridModel>) => {
        this.products = result?.Items || [];
        this.totalItems = result?.TotalItems || 0;
        this.isLoading = false;
      },
      error: () => {
        this.products = [];
        this.totalItems = 0;
        this.isLoading = false;
      }
    });
  }

  ChangeSorting(sortUrl: string): void {
    this.router.navigate(['/sort', sortUrl]);
  }
}
