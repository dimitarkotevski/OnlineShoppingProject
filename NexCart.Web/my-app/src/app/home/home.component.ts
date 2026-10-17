import { Component, OnInit } from '@angular/core';
import { ProductService } from '../customer-components/product-components/product.service';
import { ProductsFilter } from '../models/products-filter';
import { ActivatedRoute, Router } from '@angular/router';
import { ResponseListModel } from '../models/response';
import { ProductGridModel } from '../models/product-grid';


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

  /** Maps the `sort/:sort` route segment to the backend SortName value. */
  private readonly sortNames: { [key: string]: string } = {
    'popular': 'Popular',
    'newest': 'Newest',
    'best-selling': 'BestSelling',
    'price-low': 'PriceLow',
    'price-high': 'PriceHigh',
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    ) { }

  ngOnInit(): void {
    // The route drives the sort order, so the products are loaded exactly once
    // per navigation (no more duplicate request when a sorting link is clicked).
    this.route.paramMap.subscribe(params => {
      const sortUrl = params.get('sort') || 'popular';
      this.activeSorting = this.sortNames[sortUrl] || 'Popular';
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
