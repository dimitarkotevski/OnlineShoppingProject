import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { OrderViewModel } from 'src/app/admin-components/admin-tool-models/order-view-model';

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
        return this.http.post<any>(this.baseApi+'/MakeAPay',
            { model: order })
    }
}