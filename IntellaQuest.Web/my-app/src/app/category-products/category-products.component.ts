import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CategoryService } from './category.service';
import { ResponseListModel } from '../models/response';
import { ProductGridModel } from '../models/product-grid';
import { CategoryViewModel } from '../models/category';
import { RequestModel } from '../models/request';
import { ProductService } from '../customer-components/product-components/product.service';

@Component({ standalone: false,
  selector: 'app-category-products',
  templateUrl: './category-products.component.html',
  styleUrls: ['./category-products.component.css']
})
export class CategoryProductsComponent implements OnInit {
  
  //@Input() category?: Category;
  products?: ResponseListModel<ProductGridModel>;
  category?: CategoryViewModel;
  brands?: any;
  isLoading: boolean = true;
  filterCategory:RequestModel = new RequestModel();
  categoryId?: string;

  constructor(
    private route: ActivatedRoute,
    private categoryService: CategoryService,
    private productService: ProductService
    ) { }

  ngOnInit(): void {
    // Subscribe to the route exactly once; changing the side filter afterwards
    // only reloads the products (previously each filter change created a new
    // route subscription that was never unsubscribed).
    this.route.paramMap.subscribe(params => {
      const categoryId = params.get('category');
      if(!categoryId){
        return;
      }

      this.categoryId = categoryId;

      this.categoryService.GetCategoryById(categoryId).subscribe((res:any)=>{
        this.category = res;
        this.productService.GetAllBrands(res.Id)?.subscribe((brands)=>{
          this.brands = brands;
        })
      });

      this.loadProducts(this.filterCategory);
    });
  }

  refreshState(filter: RequestModel): void{
    this.loadProducts(filter);
  }

  private loadProducts(filter: RequestModel): void{
    if(!this.categoryId){
      return;
    }

    this.isLoading = true;
    this.categoryService.getProductsByCategory(this.categoryId,filter).subscribe((res : ResponseListModel<ProductGridModel>)=>{
      this.products = res;
      this.isLoading=false;
    })
  }

}

