import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {

  private baseApi = '/api/Categories/';

  constructor(private http: HttpClient) { }

  getAllCategories() {
    return this.http.post<any>(this.baseApi + 'AllCategories', null);
  }
}
