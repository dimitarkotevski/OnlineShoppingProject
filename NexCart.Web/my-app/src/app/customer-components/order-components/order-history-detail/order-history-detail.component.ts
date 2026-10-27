import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { OrderDetailViewModel } from 'src/app/models/order-detail-view-model';
import { OrderService } from '../order.service';

@Component({
  standalone: false,
  selector: 'app-order-history-detail',
  templateUrl: './order-history-detail.component.html',
  // Reuse the active-order card styles so both order views look the same.
  styleUrls: ['../order-detail/order-detail.component.css']
})
export class OrderHistoryDetailComponent implements OnInit {
  order: OrderDetailViewModel | null = null;
  isLoading = true;
  errorMessage = 'This order could not be found.';

  constructor(
    private route: ActivatedRoute,
    private orderService: OrderService,
  ) { }

  ngOnInit(): void {
    const orderId = this.route.snapshot.paramMap.get('id');
    if (!orderId) {
      this.isLoading = false;
      return;
    }

    this.orderService.GetUserOrder(orderId).subscribe({
      next: (order) => {
        this.order = order;
        this.isLoading = false;
      },
      error: (error) => {
        if (error?.status !== 404) {
          this.errorMessage = 'We could not load this order. Please try again.';
        }
        this.isLoading = false;
      }
    });
  }
}
