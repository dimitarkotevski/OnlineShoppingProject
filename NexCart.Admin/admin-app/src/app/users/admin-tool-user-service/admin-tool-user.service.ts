import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { RequestModel } from 'src/app/models/admin-tool-request-model';
import { ResponseModel } from 'src/app/models/admin-tool-response-model';
import { UserGridModel } from 'src/app/models/user-grid-model';
import { UserViewModel } from 'src/app/models/user-view-model';
import { RoleModel } from 'src/app/models/role-model';

@Injectable({
  providedIn: 'root'
})
export class AdminToolUserService {

  private baseApi = '/api/Users/';
  private rolesApi = '/api/Roles/';

  constructor(
    private http: HttpClient,
  ) { }

  getTable(request: RequestModel){
    return this.http.post<ResponseModel<UserGridModel>>(this.baseApi+"All", request);
  }
  getById(id:string){
    return this.http.post(this.baseApi+"Get",{Id:id})
  }
  getRoles(){
    return this.http.post<RoleModel[]>(this.rolesApi+"All", {})
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
  deleteMany(ids:string[]){
    return this.http.post<{ success: boolean; deleted: number }>(this.baseApi+"DeleteMany",{Ids:ids})
  }
  
}
