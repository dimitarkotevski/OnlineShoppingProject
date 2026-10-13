import { Component, OnInit } from '@angular/core';
import { ProductService } from '../customer-components/product-components/product.service';
import { ProductsFilter } from '../models/products-filter';
import { Router } from '@angular/router';
import { ResponseListModel } from '../models/response';
import { ProductGridModel } from '../models/product-grid';
import { DomSanitizer } from '@angular/platform-browser';
import { CategoryViewModel } from '../models/category';


@Component({ standalone: false,
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  isUserAuthenticated: boolean = false;
  favouriteProduct?: ProductGridModel[] | null | undefined;
  categories?: CategoryViewModel[] | null | undefined;
  products: ProductGridModel[] | null | undefined;
  search: string = 'search'
  totalItems?: number
  activeSorting?: string ='Popular';
  sortingNames?: string[];
  isLoading: boolean = true;

  message?: string="";
  type?: any="";


  productFilter?: ProductsFilter={
    SearchString:"",
    SortName: "Popular",
    PageNeeded: "",
    Price: "",
  };

  queryParam: string = "";

  constructor(
    private router: Router,
    private productService: ProductService,
    ) { }

  ngOnInit(): void {
    if(this.activeSorting==='Popular'){
      this.router.navigate(['sort/popular']);
    }

    this.productService.getAll(this.productFilter).subscribe((result:ResponseListModel<ProductGridModel>) => {
          this.products = result.Items;
          this.totalItems = result.TotalItems;
          this.isLoading = false;
      });
  }


  ChangeSorting(sorting: string): void {
    this.activeSorting = sorting;
    this.productFilter ={
      SearchString:"",
      SortName: sorting,
      PageNeeded: "",
      Price: "",
    };
    this.productService.getAll(this.productFilter).subscribe(products => {
      this.products = products.Items;
      this.totalItems = products.recordsTotal;
    });
  }
  ChangeRoute(url: string): void {
    this.router.navigate(["sort/"+url]);
  }


  addToFavourite(Id:string | undefined){
    this.message = 'Success message';
    this.type = 'success';
  }
}

