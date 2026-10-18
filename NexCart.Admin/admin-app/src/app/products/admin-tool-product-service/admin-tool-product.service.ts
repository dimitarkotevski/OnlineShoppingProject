import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { RequestModel } from 'src/app/models/admin-tool-request-model';
import { ResponseModel } from 'src/app/models/admin-tool-response-model';
import { ProductGridModel } from 'src/app/models/product-grid';
import { ProductViewModel } from 'src/app/models/product-view-model';

@Injectable({
  providedIn: 'root'
})
export class AdminToolProductService {

  private baseApi = '/api/Products/';

  constructor(
    private http: HttpClient,
  ) { }

  getTable(request: RequestModel){
    return this.http.post<ResponseModel<ProductGridModel>>(this.baseApi+"All", request);
  }
  getById(id:string){
    return this.http.post(this.baseApi+"Get",{Id:id})
  }
  create(model: ProductViewModel){
    return this.http.post(this.baseApi+"Create",{model:model})
  }
  update(model: ProductViewModel){
    return this.http.post(this.baseApi+"Edit",{model:model})
  }
  delete(id:string){
    return this.http.post(this.baseApi+"Delete",{Id:id})
  }
  deleteMany(ids:string[]){
    return this.http.post<{ success: boolean; deleted: number }>(this.baseApi+"DeleteMany",{Ids:ids})
  }
  addImageToProduct(id:string, base64: string){
    return this.http.post<string | boolean>(this.baseApi+"AddImageToProduct", {id:id, image:base64});
  }
}
