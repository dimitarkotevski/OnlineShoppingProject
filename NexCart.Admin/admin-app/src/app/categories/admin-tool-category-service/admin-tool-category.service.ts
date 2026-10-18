import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { RequestModel } from 'src/app/models/admin-tool-request-model';
import { ResponseModel } from 'src/app/models/admin-tool-response-model';
import { CategoryEditModel, CategoryGridModel } from 'src/app/models/category-grid';

@Injectable({
  providedIn: 'root'
})
export class AdminToolCategoryService {

  private baseApi = '/api/Categories/';

  constructor(
    private http: HttpClient,
  ) { }

  getTable(request: RequestModel){
    return this.http.post<ResponseModel<CategoryGridModel>>(this.baseApi+"All", request);
  }
  create(model: CategoryEditModel){
    return this.http.post(this.baseApi+"Create",{model:model})
  }
  update(model: CategoryEditModel){
    return this.http.post(this.baseApi+"Edit",{model:model})
  }
  delete(id:string){
    return this.http.post(this.baseApi+"Delete",{Id:id})
  }
}
