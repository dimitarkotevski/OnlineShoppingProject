import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';


@Injectable({
  providedIn: 'root'
})
export class ProductService {
    private baseApi = "api/Products/";

  constructor(private http: HttpClient) { }

  getAll(filter:any) {
    return this.http.post<any>(this.baseApi+'All',filter)
  }
  get(Id:string) {
    return this.http.post<any>(this.baseApi+'Get/'+Id,JSON)
  }
  GetAllBrands(categoryId:string){
    if(categoryId){
      return this.http.post<string[]>(this.baseApi+"GetAllBrands?categoryId="+categoryId, null);
    }else{
      return null;
    }
  }

}