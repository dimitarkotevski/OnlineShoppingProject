import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { RequestModel } from "../models/request";

@Injectable({
    providedIn: 'root'
  })
export class CategoryService{
    
    private baseApi = "api/Categories/";

    constructor(private http: HttpClient) { }

    getAllCategories(){
        return this.http.post<any>(this.baseApi+'AllCategories',null)
    }
    getProductsByCategory(Id:string,filter : RequestModel){
        return this.http.post<any>(this.baseApi+'GetProductsByCategory/'+Id,
            filter)
    }

    GetCategoryById(Id:string){
        return this.http.post<any>(this.baseApi+'GetCategoryById/'+Id,
            null)
    }
}