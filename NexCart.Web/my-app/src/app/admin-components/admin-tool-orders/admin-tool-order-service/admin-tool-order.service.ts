import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { RequestModel } from '../../admin-tool-models/admin-tool-request-model';
import { OrderGridViewModel } from 'src/app/models/order-grid';
import { ResponseModel } from '../../admin-tool-models/admin-tool-response-model';
import { OrderViewModel } from '../../admin-tool-models/order-view-model';

@Injectable({
  providedIn: 'root'
})
export class AdminToolOrderService {

  private baseApi = 'api/Orders/';

  constructor(
    private http: HttpClient,
  ) { }

  getTable(request: RequestModel){
    return this.http.post<ResponseModel<OrderGridViewModel>>(this.baseApi+"All", request);
  }
  getById(id:string){
    return this.http.post<OrderViewModel>(this.baseApi+"Get",{Id:id})
  }
  create(model: OrderViewModel){
    return this.http.post(this.baseApi+"Create",{model:model})
  }
  update(model: OrderViewModel){
    return this.http.post(this.baseApi+"Edit",{model:model})
  }
  delete(id:string){
    return this.http.post<OrderViewModel>(this.baseApi+"Delete",{Id:id})
  }
}
