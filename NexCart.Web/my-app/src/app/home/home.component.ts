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

  /** Products in the order the backend returned them; sorting is applied on a copy. */
  private allProducts: ProductGridModel[] = [];
  private productsLoaded: boolean = false;

  /** Maps the `sort/:sort` route segment to the sort applied on the client. */
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
    // The route drives the sort order. Products are fetched once and re-sorted
    // locally on every sorting change.
    this.route.paramMap.subscribe(params => {
      const sortUrl = params.get('sort') || 'popular';
      this.activeSorting = this.sortNames[sortUrl] || 'Popular';
      if (this.productsLoaded) {
        this.applySorting();
      } else {
        this.loadProducts();
      }
    });
  }

  private loadProducts(): void {
    const filter: ProductsFilter = {
      SearchString: "",
      PageNeeded: "",
      Price: "",
    };

    this.isLoading = true;
    this.productService.getAll(filter).subscribe({
      next: (result: ResponseListModel<ProductGridModel>) => {
        this.allProducts = result?.Items || [];
        this.totalItems = result?.TotalItems || 0;
        this.productsLoaded = true;
        this.applySorting();
        this.isLoading = false;
      },
      error: () => {
        this.allProducts = [];
        this.products = [];
        this.totalItems = 0;
        this.isLoading = false;
      }
    });
  }

  /**
   * Sorts a copy of the loaded products. Sorts the client has no data for
   * (Popular, BestSelling) keep the order the backend returned.
   */
  private applySorting(): void {
    const sorted = [...this.allProducts];
    const price = (p: ProductGridModel) => p.Price ?? 0;
    const created = (p: ProductGridModel) => (p.DateCreated ? Date.parse(p.DateCreated) : 0) || 0;

    switch (this.activeSorting) {
      case 'Newest':
        sorted.sort((a, b) => created(b) - created(a));
        break;
      case 'PriceLow':
        sorted.sort((a, b) => price(a) - price(b));
        break;
      case 'PriceHigh':
        sorted.sort((a, b) => price(b) - price(a));
        break;
    }

    this.products = sorted;
  }

  ChangeSorting(sortUrl: string): void {
    this.router.navigate(['/sort', sortUrl]);
  }
}
