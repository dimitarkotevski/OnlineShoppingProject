import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ProductGridModel } from '../../../models/product-grid';
import { ToastrService } from 'ngx-toastr';
import { AuthentificationService } from '../../../authentification/authentification.service';
import { ResponseListModel } from '../../../models/response';
import { FavouriteProductService } from 'src/app/favourite/favourite-product.service';
import { ShoppingCartService } from 'src/app/customer-components/cart-components/shopping-cart.service';
import { FavouriteComponent } from 'src/app/favourite/favourite.component';
import { Router } from '@angular/router';


@Component({ standalone: false,
  selector: 'app-products-grid',
  templateUrl: './products-grid.component.html',
  styleUrls: ['./products-grid.component.css']
})
export class ProductsGridComponent implements OnInit {

  @Input() hasRemoveProductButton?: boolean;
  @Input() products: ProductGridModel[] | null | undefined;
  @Input() orderProducts: any;
  @Input() shoppingCart: any | null | undefined;
  @Input() type: "home" | "cart" | "favourite" | "order" | undefined;
  @Output() refreshState = new EventEmitter<any>();

  @Input() isLoading: boolean = false;
  @Input() emptyTitle: string = 'No Products in Database';
  @Input() emptyMessage: string = 'There are currently no products available in the database.';
  @Input() emptyIcon: string = 'fa-solid fa-box-open';

  // Keep the cart UI in sync with ShoppingCartService's server-side limit.
  readonly maxQuantityPerLine = 999;

  get cartItemCount(): number {
    return (this.shoppingCart?.ShoppingCartDetails ?? []).reduce(
      (count: number, detail: any) => count + (Number(detail?.Quantity) || 0),
      0
    );
  }

  get cartTotal(): number {
    return (this.shoppingCart?.ShoppingCartDetails ?? []).reduce(
      (total: number, detail: any) =>
        total +
        (Number(detail?.Product?.Price) || 0) *
          (Number(detail?.Quantity) || 0),
      0
    );
  }
  
  constructor(
    private router: Router,
    private toastr: ToastrService,
    private authService: AuthentificationService,
    private favouriteProductService: FavouriteProductService,
    private shoppingCartService: ShoppingCartService
    ) { 
        if(this.type === undefined || this.type == undefined){
          this.type = 'home';
        }
      }

  ngOnInit(): void {

  }

  addToFavourite(productId: string | undefined){
    var userId = this.authService.getLoggedUserId();
    if(!userId){
      window.location.replace('/login')
    }
    if(productId){
      this.favouriteProductService.addFavouriteProducts(userId,productId).subscribe(
      ()=>{
        this.toastr.success("Product added!");
      },
      ()=>{
        this.toastr.warning("Product already in favourite products!");
      });
    }else{
      this.toastr.error("Product can't be added!");
    }
  }

  addToCart(productId: string | undefined){
    var userId = this.authService.getLoggedUserId();
    if(!userId || !productId){
      window.location.replace('/login')
    }else{
      this.shoppingCartService.AddProductToCart(this.authService.getLoggedUserId(),productId || null)?.subscribe(()=>{
        this.toastr.success("Successs added product to cart");
      })
    }
  }

  deleteFavouriteProduct(id:string | undefined){
    if(id){
      this.favouriteProductService.deleteFavouriteProduct(this.authService.getLoggedUserId(),id)?.subscribe(()=>{
        this.toastr.success("Success deleted favourite product");
        this.refresh();
      })
    }
  }



  handleMinus(i:number){
    const detail = this.shoppingCart?.ShoppingCartDetails?.[i];
    if(detail && detail.Quantity > 1){
      detail.Quantity--;
      this.shoppingCartService.MinusQuantity(detail.Id)?.subscribe();
    }
  }

  handlePlus(i:number){
    const detail = this.shoppingCart?.ShoppingCartDetails?.[i];
    if(detail && detail.Quantity < this.maxQuantityPerLine){
      detail.Quantity++;
      this.shoppingCartService.PlusQuantity(detail.Id)?.subscribe();
    }
  }

  deleteProductCartDetail(id:string){
    if(id){
      this.shoppingCartService.RemoveCartDetail(id)?.subscribe(()=>{
        this.toastr.success("Success remove cart product")
        this.refresh();
      });
    }else{
      this.toastr.error("Something is wrong")
    }
  }




  refresh() {
    const that = this;
    if (that.refreshState) {
      that.refreshState.emit();
    }
  }
}

