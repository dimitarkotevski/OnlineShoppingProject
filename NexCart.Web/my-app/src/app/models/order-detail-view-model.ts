import { OrderStatus } from "./enum/order-status";
import { PaymentType } from "./enum/payment-type";
import { ProductViewModel } from "./product-view-model";

export class OrderLineViewModel {
    Id?: string;
    Product?: ProductViewModel;
    Quantity?: number;
}

export class OrderDetailViewModel {
    Id?: string;
    OrderName?: string;
    Products?: OrderLineViewModel[];
    OrderStatus?: OrderStatus;
    PaymentType?: PaymentType;
    TotalAmount?: number;
    DateCreated?: string;
}
