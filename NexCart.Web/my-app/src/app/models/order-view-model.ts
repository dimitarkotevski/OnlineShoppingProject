import { OrderStatus } from "src/app/models/enum/order-status"
import { PaymentType } from "src/app/models/enum/payment-type"
import { LookupViewModel } from "src/app/models/lookup-view-model"

export class OrderViewModel{
    Id?: string
    OrderName?:string
    ShoppingCart?: LookupViewModel
    User?: LookupViewModel
    OrderStatus?: OrderStatus
    TotalAmount?: number
    PaymentType?: PaymentType
    DateCreated?: string | undefined
}