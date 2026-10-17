import { Component, Input, OnInit } from '@angular/core';
import { AuthentificationService } from '../../../authentification/authentification.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ShoppingCartService } from 'src/app/customer-components/cart-components/shopping-cart.service';

@Component({ standalone: false,
  selector: 'app-cart-grid',
  templateUrl: './cart-grid.component.html',
  styleUrls: ['./cart-grid.component.css']
})
export class CartGridComponent implements OnInit {

  @Input() shoppingCart?: any;
  isLoading: boolean = true;

  constructor(
    private authService: AuthentificationService,
    private shoppingCartService: ShoppingCartService,
    private router: Router,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    if(!this.authService.isAuthenticated()){
      this.router.navigate(["login"]);
    }

    this.shoppingCartService.GetUserCartProducts(this.authService.getLoggedUserId())?.subscribe((res)=>{
      this.shoppingCart = res;
      this.isLoading=false;
    })

  }

  handleMinus(i:number){
    if(this.shoppingCart.ShoppingCartDetails[i].Quantity>1){
      this.shoppingCart.ShoppingCartDetails[i].Quantity--;
      this.shoppingCartService.MinusQuantity(this.shoppingCart.ShoppingCartDetails[i].Id)?.subscribe();
    }
  }
  handlePlus(i:number){
    this.shoppingCart.ShoppingCartDetails[i].Quantity++;
    this.shoppingCartService.PlusQuantity(this.shoppingCart.ShoppingCartDetails[i].Id)?.subscribe();
  }

  deleteProductCartDetail(id:string){
    if(id){
      this.shoppingCartService.RemoveCartDetail(id)?.subscribe(()=>{

        this.shoppingCartService.GetUserCartProducts(this.authService.getLoggedUserId())?.subscribe((res)=>{
          this.shoppingCart = res;
        })

        this.toastr.success("Success remove cart product")
      });
    }else{
      this.toastr.error("Something is wrong")
    }
  }

}

