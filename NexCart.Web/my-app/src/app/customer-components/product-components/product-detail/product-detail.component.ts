import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../product.service';
import { ProductGridModel } from '../../../models/product-grid';
import { ProductViewModel } from 'src/app/models/product-view-model';
import { AuthentificationService } from '../../../authentification/authentification.service';
import { FavouriteProductService } from 'src/app/favourite/favourite-product.service';
import { ShoppingCartService } from 'src/app/customer-components/cart-components/shopping-cart.service';
import { ToastrService } from 'ngx-toastr';

@Component({ standalone: false,
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css']
})
export class ProductDetailComponent implements OnInit {

  productDetail?: ProductViewModel

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private favouriteProductService: FavouriteProductService,
    private shoppingCartService: ShoppingCartService,
    private authService: AuthentificationService,
    private toastr: ToastrService
    ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if(id){
        this.productService.get(id).subscribe( (result: ProductGridModel)=>{
          this.productDetail = result;
        })
      }
    });
  }

  addToFavourite(Id:string | undefined){
    const userId = this.authService.getLoggedUserId();
    if(!userId){
      window.location.replace('/login');
      return;
    }
    if(!Id){
      this.toastr.error("Product can't be added to favourite");
      return;
    }
    this.favouriteProductService.addFavouriteProducts(userId, Id).subscribe(
      () => this.toastr.success("Product added to favourite"),
      () => this.toastr.warning("Product is already in favourite")
    );
  }

  /** Adds the product to the cart and goes straight to the cart to check out. */
  buyNow(Id:string | undefined){
    const userId = this.authService.getLoggedUserId();
    if(!userId || !Id){
      window.location.replace('/login');
      return;
    }
    this.shoppingCartService.AddProductToCart(userId, Id)?.subscribe(
      () => this.router.navigate(['/cart'])
    );
  }

  addToCart(Id:string | undefined){
    const userId = this.authService.getLoggedUserId();
    if(!userId || !Id){
      window.location.replace('/login');
      return;
    }
    this.shoppingCartService.AddProductToCart(userId, Id)?.subscribe(
      () => this.toastr.success("Product added to cart")
    );
  }
}
