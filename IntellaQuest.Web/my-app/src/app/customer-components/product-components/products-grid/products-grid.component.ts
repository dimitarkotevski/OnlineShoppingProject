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

  isLoading: boolean = true;
  
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
    if(this.shoppingCart.ShoppingCartDetails[i].Quantity>1){
      this.shoppingCart.ShoppingCartDetails[i].Quantity--;
      this.shoppingCartService.MinusQuantity(this.shoppingCart.ShoppingCartDetails[i].Id)?.subscribe(res=>{

      })
    }
  }
  handlePlus(i:number){
    this.shoppingCart.ShoppingCartDetails[i].Quantity++;
    this.shoppingCartService.PlusQuantity(this.shoppingCart.ShoppingCartDetails[i].Id)?.subscribe((res)=>{

    })
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

