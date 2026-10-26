import { Component, Input, OnInit } from '@angular/core';
import { AuthentificationService } from 'src/app/authentification/authentification.service';
import { ResponseListModel } from '../../../models/response';
import { OrderGridViewModel } from '../../../models/order-grid';
import { OrderService } from '../order.service';
import { OrderStatus } from 'src/app/models/enum/order-status';

@Component({ standalone: false,
  selector: 'app-order-grid',
  templateUrl: './order-grid.component.html',
  styleUrls: ['./order-grid.component.css']
})
export class OrderGridComponent implements OnInit {
  /** When embedded under the active-order card, hide the page chrome. */
  @Input() embedded = false;

  orders?: OrderGridViewModel[] | null;
  isLoading: boolean = true;

  constructor(
    private orderService: OrderService,
    private authService: AuthentificationService,
  ) { }

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.orderService.GetUserNotActiveOrders(this.authService.getLoggedUserId()).subscribe({
      next: (res: ResponseListModel<OrderGridViewModel>) => {
        this.orders = res.Items ?? [];
        this.isLoading = false;
      },
      error: () => {
        this.orders = [];
        this.isLoading = false;
      }
    });
  }

  statusClass(status?: OrderStatus): string {
    switch (status) {
      case OrderStatus.Canceled: return 'status-pill--danger';
      case OrderStatus.OnHold: return 'status-pill--warning';
      case OrderStatus.PendingPayment: return 'status-pill--pending';
      case OrderStatus.Completed: return 'status-pill--success';
      default: return '';
    }
  }

  statusIcon(status?: OrderStatus): string {
    switch (status) {
      case OrderStatus.Canceled: return 'fa-circle-xmark';
      case OrderStatus.OnHold: return 'fa-circle-pause';
      case OrderStatus.PendingPayment: return 'fa-clock';
      case OrderStatus.Completed: return 'fa-circle-check';
      default: return 'fa-circle';
    }
  }

}

