import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';
import { UserTokenInfo } from '../models/login/user-token-info';
import { UserDetails } from '../models/login/user-details';
import { UserRegister } from '../models/register/register-user';
import { ChangePasswordViewModel } from '../models/change-password-view-model';
import { PaymentInfoViewModel } from '../models/payment-info-view-model';


@Injectable({
  providedIn: 'root'
})
export class AuthentificationService {
  

  private baseApi = 'api/Identity';

  constructor(
    private http: HttpClient,
    private tokenHelper: JwtHelperService
    ) {}
    
  getUserDetails(id:string | null) : Observable<UserDetails>{
    return this.http.post<UserDetails>(`${this.baseApi}/GetUserDetails/${id}`, null);
  }
  updateUserDetails(userDetails:UserDetails){
    return this.http.post(`${this.baseApi}/SetUserDetails`, {model:userDetails});
  }
  getLoggedUsername() : string | undefined {
    return localStorage.getItem('username') || undefined;
  }

  login(username:string,password:string): Observable<boolean> {
    return this.http.post<any>(this.baseApi+"/Login", {Username:username, Password:password}).pipe(
      map((response : UserTokenInfo) => {
        if (response.Id && response.Token && response.Username && response.Role) {
          localStorage.setItem('id', response.Id);
          localStorage.setItem('username', response.Username);
          localStorage.setItem('token', response.Token);
          localStorage.setItem('role',response.Role);
          return true;
        }
        return false;
      })
    );
  }
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('id');
    localStorage.removeItem('username');
    localStorage.removeItem('role');
  }
  getLoggedUserId(): string | null {
    return localStorage.getItem('id') || null;
  }
  getRole(){
    return localStorage.getItem('role') || null;
  }
  getToken(): string | null  {
    return localStorage.getItem('token') || null;
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return token !== null;
  }
  getTokenInformation(){
    if(this.isAuthenticated()){
      return this.tokenHelper.decodeToken(this.getToken() || '') || null;
    }
    return null;
  }

  registerUser(userRegister: UserRegister): Observable<boolean> {
    // Registration only returns a boolean on success (no token is issued here).
    return this.http.post<boolean>(this.baseApi+"/Registration", {model:userRegister});
  }

  getAmountMoneyOfUser(userId: string | null){
    if(userId){
      return this.http.post(`${this.baseApi}/GetAmountMoneyOfUser`, { userId : userId });
    }else{
      return null;
    }
  }

  changePassword(changepasswordModel :ChangePasswordViewModel){
    return this.http.post(`${this.baseApi}/ChangePassword`, {model:changepasswordModel});
  }

  addPayment(model : PaymentInfoViewModel) {
    return this.http.post(`${this.baseApi}/AddPayment`, { userId: this.getLoggedUserId()  ,model : model});
  }

  deletePayment(userId: string) {
    return this.http.post(`${this.baseApi}/DeletePayment`, {userId : userId});
  }

}