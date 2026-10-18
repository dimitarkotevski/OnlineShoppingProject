import { Component, Input, OnInit } from '@angular/core';
import { OrderService } from '../order.service';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { YesNoDialogComponent } from '../../yes-no-dialog/yes-no-dialog.component';
import { MakeOrderConfirmationComponent } from './make-order-confirmation/make-order-confirmation.component';
import { OrderViewModel } from 'src/app/models/order-view-model';
import { LookupViewModel } from 'src/app/models/lookup-view-model';
import { AuthentificationService } from 'src/app/authentification/authentification.service';
import { ToastrService } from 'ngx-toastr';

@Component({ standalone: false,
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.css']
})
export class OrderDetailComponent implements OnInit {

  @Input() activeOrder?: any;

  constructor(
    private dialog: MatDialog,
    private orderService: OrderService,
    private toasterService: ToastrService,
    private authentificationService: AuthentificationService,
  ) { }
  ngOnInit(): void {

  }

  Confirmation(order: OrderViewModel){
    const dialogRef = this.dialog.open(MakeOrderConfirmationComponent, {
      data: { order },
      width: '500px', // Set the width of the modal
      panelClass: 'custom-modalbox'
    });
    dialogRef.afterClosed().subscribe((result) => {
      if(result){
         this.orderService.MakeAPay(order).subscribe(()=>{
            this.activeOrder = null
         })
         this.toasterService.success("Your order has been aproved and the product/s will be sent to the address that you put into profile")
      }
    });
  }

  CancelOrder(orderId:string){
    const text = "Do you want to cancel this order?"
    const dialogRef = this.dialog.open( YesNoDialogComponent, {
      data: { text },
      width: '500px', // Set the width of the modal
      panelClass: 'custom-modalbox'
    });

    dialogRef.afterClosed().subscribe((result) => {
      if(result){
        this.orderService.CancelActiveOrder(orderId).subscribe(()=>{
          window.location.replace("my-order");
        })
      }
    });
  }
}

