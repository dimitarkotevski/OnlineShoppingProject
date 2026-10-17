import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { RequestModel } from '../../admin-tool-models/admin-tool-request-model';
import { ResponseModel } from '../../admin-tool-models/admin-tool-response-model';
import { UserGridModel } from 'src/app/models/user-grid-model';
import { UserViewModel } from 'src/app/models/user-view-model';

@Injectable({
  providedIn: 'root'
})
export class AdminToolUserService {

  private baseApi = 'api/Users/';

  constructor(
    private http: HttpClient,
  ) { }

  getTable(request: RequestModel){
    return this.http.post<ResponseModel<UserGridModel>>(this.baseApi+"All", request);
  }
  getById(id:string){
    return this.http.post(this.baseApi+"Get",{Id:id})
  }
  create(model: UserViewModel){
    return this.http.post(this.baseApi+"Create",{model:model})
  }
  update(model: UserViewModel){
    return this.http.post(this.baseApi+"Update",{model:model})
  }
  delete(id:string){
    return this.http.post(this.baseApi+"Delete",{Id:id})
  }
  
}
