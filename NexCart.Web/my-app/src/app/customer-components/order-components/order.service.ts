import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { OrderViewModel } from 'src/app/models/order-view-model';
import { OrderStatus } from 'src/app/models/enum/order-status';
import { OrderDetailViewModel } from 'src/app/models/order-detail-view-model';
import { SKIP_ERROR_TOAST } from 'src/app/utils/error.interceptor';

@Injectable({
    providedIn: 'root'
  })
export class OrderService{
    private baseApi = "api/Orders";

    constructor(private http: HttpClient) { }

    GetUserNotActiveOrders(userId:string | null) {
        return this.http.post<any>(this.baseApi+'/GetUserNotActiveOrders',{userId: userId})
    }

    GetUserActiveOrder(userId:string | null) {
        return this.http.post<any>(this.baseApi+'/GetUserActiveOrder',{userId: userId})
    }

    GetUserOrder(orderId: string) {
        return this.http.get<OrderDetailViewModel>(this.baseApi+'/GetUserOrder', {
            params: { orderId },
            // The detail page renders its own "not found" state.
            context: new HttpContext().set(SKIP_ERROR_TOAST, true)
        })
    }
    CancelActiveOrder(orderId:string){
        return this.http.post<any>(this.baseApi+'/CancelActiveOrder',{orderId: orderId})
    }

    MakeAnOrder(shoppingCartId : string,userId : string){
        return this.http.post<any>(this.baseApi+'/MakeAnOrder',
            {shoppingCartId: shoppingCartId,
                userId: userId
            })
    }

    MakeAPay(order : OrderViewModel){
        return this.http.post<{ success: boolean; orderStatus: OrderStatus }>(
            this.baseApi+'/MakeAPay',
            { model: order })
    }
}