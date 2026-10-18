import { Pipe, PipeTransform } from '@angular/core';
import { OrderStatus } from 'src/app/models/enum/order-status';

@Pipe({ standalone: false,
  name: 'orderStatus'
})
export class OrderStatusPipe implements PipeTransform {
  transform(value: OrderStatus | undefined): string{
    switch (value) {
      case OrderStatus.Canceled:
        return 'Canceled';
      case OrderStatus.OnHold:
        return 'On Hold';
      case OrderStatus.PendingPayment:
        return 'Pending Payment';
      case OrderStatus.Completed:
        return 'Completed';
      default:
        return '';
    }
  }
}
