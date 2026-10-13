import { Component, Input, OnInit } from '@angular/core';
import { AuthentificationService } from '../authentification/authentification.service';
import { OrderService } from '../customer-components/order-components/order.service';
import { ShoppingCartService } from '../customer-components/cart-components/shopping-cart.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { YesNoDialogComponent } from '../customer-components/yes-no-dialog/yes-no-dialog.component';

@Component({ standalone: false,
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit {
  
  @Input() shoppingCart?: any;

  isLoading: boolean = true;
  error: string = "";

  constructor(
    private router: Router,
    private dialog: MatDialog,
    private toastr: ToastrService,
    private orderService: OrderService,
    private authService: AuthentificationService,
    private shoppingCartService: ShoppingCartService,
  ) { }

  ngOnInit(): void {
    if(!this.authService.isAuthenticated()){
      this.router.navigate(["login"]);
    }

    this.refreshState();

  }

  refreshState(): void {
    this.shoppingCartService.GetUserCartProducts(this.authService.getLoggedUserId())?.subscribe((res) => {
      this.shoppingCart = res;
      this.isLoading = false;
    },(error)=>{
      this.error = error;
      this.isLoading = false;
    });
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

  MakeOrder(shoppingCartId:string){
    let text = "Do you want to make an order?"
    const dialogRef = this.dialog.open(YesNoDialogComponent, {
      data: { text },
      width: '500px', // Set the width of the modal
      panelClass: 'custom-modalbox'
    });
    dialogRef.afterClosed().subscribe((result) => {
      if(result){
        this.orderService.MakeAnOrder(shoppingCartId,this.authService.getLoggedUserId() || "").subscribe(()=>{
          window.location.replace("my-order");
        })
      }
    });
  }

}

