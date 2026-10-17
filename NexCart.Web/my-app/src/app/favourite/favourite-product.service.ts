import { HttpClient, HttpContext } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, of } from "rxjs";
import { SKIP_ERROR_TOAST } from "../utils/error.interceptor";

@Injectable({
    providedIn: 'root'
})
export class FavouriteProductService{

    private baseApi = "api/FavouriteProducts/";

    constructor(private http: HttpClient) { }


      getFavouriteProducts(Id:string | null): Observable<any>  {
        return this.http.post<any>(this.baseApi+'GetUserFavouriteProducts',{userId: Id})
      }
    
      addFavouriteProducts(userId:string | null, productId: string | null): Observable<any>  {
        if(userId && productId){
          // A duplicate favourite comes back as an error; the caller shows a friendlier
          // "already in favourites" warning, so keep the global toastr quiet here.
          return this.http.post<any>(this.baseApi+'UserAddFavouriteProduct',{userId: userId, productId: productId}, {
            context: new HttpContext().set(SKIP_ERROR_TOAST, true),
          })
        }
        else{
          return of(null);
        }
      }
      deleteFavouriteProduct(userId:string | null, productId: string| null){
        if(userId && productId){
          return this.http.post<any>(this.baseApi+'RemoveFavouriteProduct',{userId: userId, productId: productId})
        }else{
          return null;
        }
      }
}