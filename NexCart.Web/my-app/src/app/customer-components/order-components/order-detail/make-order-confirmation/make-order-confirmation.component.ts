import { Component, Inject, Input, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { OrderViewModel } from 'src/app/admin-components/admin-tool-models/order-view-model';

@Component({ standalone: false,
  selector: 'app-make-order-confirmation',
  templateUrl: './make-order-confirmation.component.html',
  styleUrls: ['./make-order-confirmation.component.css']
})
export class MakeOrderConfirmationComponent implements OnInit {

  @Input() orderViewModel : OrderViewModel;

  constructor(
    @Inject(
      MAT_DIALOG_DATA) public data:any
      ) { 
        this.orderViewModel = data.order
      }

  ngOnInit(): void {
  }

}

