import { OrderStatus } from "./enum/order-status";
import { PaymentType } from "./enum/payment-type";
import { LookupViewModel } from "./lookup-view-model";

export class OrderGridViewModel{
    Id?:string;
    OrderName?: string;
    OrderStatus?: OrderStatus;
    ShoppingCart?: LookupViewModel;
    NumberOfProducts?: number;
    TotalAmount?: number;
    DateCreated?: string;
    PaymentType?: PaymentType
    //if we need
    User?: LookupViewModel;
}