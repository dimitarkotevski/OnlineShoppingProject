import { Component, OnInit, ViewChild } from '@angular/core';
import { AuthentificationService } from '../authentification/authentification.service';
import { OrderGridComponent } from '../customer-components/order-components/order-grid/order-grid.component';
import { OrderService } from '../customer-components/order-components/order.service';

@Component({ standalone: false,
  selector: 'app-order',
  templateUrl: './order.component.html',
  styleUrls: ['./order.component.css']
})
export class OrderComponent implements OnInit {

  activeOrder?: any;
  @ViewChild(OrderGridComponent) orderGrid?: OrderGridComponent;

  constructor(
    private orderService: OrderService,
    private authService: AuthentificationService,
  ) { }

  ngOnInit(): void {
    this.loadActiveOrder();
  }

  loadActiveOrder(): void {
    this.orderService.GetUserActiveOrder(this.authService.getLoggedUserId()).subscribe({
      next: (response) => {
        this.activeOrder = response;
      },
      error: () => {
        this.activeOrder = null;
      }
    });
  }

  onOrderPlaced(): void {
    this.loadActiveOrder();
    this.orderGrid?.loadOrders();
  }

}

