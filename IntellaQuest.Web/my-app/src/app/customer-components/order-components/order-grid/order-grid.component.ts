import { Component, OnInit } from '@angular/core';
import { AuthentificationService } from 'src/app/authentification/authentification.service';
import { ResponseListModel } from '../../../models/response';
import { OrderGridViewModel } from '../../../models/order-grid';
import { OrderService } from '../order.service';

@Component({ standalone: false,
  selector: 'app-order-grid',
  templateUrl: './order-grid.component.html',
  styleUrls: ['./order-grid.component.css']
})
export class OrderGridComponent implements OnInit {
  orders?: OrderGridViewModel[] | null;
  isLoading: boolean = true;

  constructor(
    private orderService: OrderService,
    private authService: AuthentificationService,
  ) { }

  ngOnInit(): void {
    this.orderService.GetUserNotActiveOrders(this.authService.getLoggedUserId()).subscribe({
      next: (res: ResponseListModel<OrderGridViewModel>) => {
        this.orders = res.Items;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

}

