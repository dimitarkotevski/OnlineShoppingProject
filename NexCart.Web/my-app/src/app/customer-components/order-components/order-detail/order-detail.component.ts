import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { OrderService } from '../order.service';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { YesNoDialogComponent } from '../../yes-no-dialog/yes-no-dialog.component';
import { MakeOrderConfirmationComponent } from './make-order-confirmation/make-order-confirmation.component';
import { OrderViewModel } from 'src/app/models/order-view-model';
import { LookupViewModel } from 'src/app/models/lookup-view-model';
import { AuthentificationService } from 'src/app/authentification/authentification.service';
import { OrderStatus } from 'src/app/models/enum/order-status';
import { ToastrService } from 'ngx-toastr';

@Component({ standalone: false,
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.css']
})
export class OrderDetailComponent implements OnInit {

  @Input() activeOrder?: any;
  @Output() orderPlaced = new EventEmitter<void>();

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
    dialogRef.afterClosed().subscribe((result: OrderViewModel | null) => {
      if(result){
         // The dialog returns the order with the chosen payment method.
         this.orderService.MakeAPay(result).subscribe((response)=>{
            this.activeOrder = null;

            if (response?.orderStatus === OrderStatus.Completed) {
              this.toasterService.success("Payment successful. Your order will be sent to the address in your profile.");
            } else {
              this.toasterService.info("Your order has been placed. It is waiting for payment.");
            }

            this.orderPlaced.emit();
         })
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

