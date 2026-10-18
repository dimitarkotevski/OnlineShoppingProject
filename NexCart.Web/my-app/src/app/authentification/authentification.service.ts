import { Injectable } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { Observable, map, of } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';
import { SKIP_ERROR_TOAST } from '../utils/error.interceptor';
import { UserTokenInfo } from '../models/login/user-token-info';
import { UserDetails } from '../models/login/user-details';
import { UserRegister } from '../models/register/register-user';
import { ChangePasswordViewModel } from '../models/change-password-view-model';
import { PaymentInfoViewModel } from '../models/payment-info-view-model';

/**
 * Candidate claim names for each piece of identity. Both the short JWT names and the long .NET
 * claim URIs are listed because the outbound claim-type mapping decides which one is emitted.
 */
const USERNAME_CLAIMS = [
  'unique_name',
  'name',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
];
const USER_ID_CLAIMS = [
  'nameid',
  'sub',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
];
const ROLE_CLAIMS = [
  'role',
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
];

@Injectable({
  providedIn: 'root'
})
export class AuthentificationService {
  

  private baseApi = 'api/Identity';

  constructor(
    private http: HttpClient,
    private tokenHelper: JwtHelperService
    ) {
    // Identity now lives in the signed token. Drop any keys written by older builds so a stale
    // plaintext role/id/username cannot linger in the browser.
    localStorage.removeItem('id');
    localStorage.removeItem('username');
    localStorage.removeItem('role');

    // Administrators may not use the shop (they sign in to the admin tool at /admin). Drop an
    // admin session left over from before that rule, before anything reads claims from it.
    const token = this.getToken();
    if (token && this.isAdminToken(token)) {
      this.logout();
    }
  }
    
  getUserDetails(id:string | null) : Observable<UserDetails>{
    return this.http.post<UserDetails>(`${this.baseApi}/GetUserDetails/${id}`, null);
  }
  updateUserDetails(userDetails:UserDetails){
    return this.http.post(`${this.baseApi}/SetUserDetails`, {model:userDetails});
  }
  /**
   * Identity (id, username, role) is read from the signed JWT instead of being duplicated in
   * localStorage. The token is verified by the API, so these values cannot be tampered with on
   * the client: editing them would break the signature and the API would reject the token.
   */
  getLoggedUsername() : string | undefined {
    return this.getClaim(USERNAME_CLAIMS) || undefined;
  }

  login(username:string,password:string): Observable<boolean> {
    // The login page shows its own toast for failures, so keep the global toastr quiet.
    return this.http.post<any>(this.baseApi+"/Login", {Username:username, Password:password}, {
      context: new HttpContext().set(SKIP_ERROR_TOAST, true),
    }).pipe(
      map((response : UserTokenInfo) => {
        if (response.Token) {
          // Only the token is stored; everything else is derived from it on demand.
          localStorage.setItem('token', response.Token);
          return true;
        }
        return false;
      })
    );
  }
  logout(): void {
    localStorage.removeItem('token');
  }
  getLoggedUserId(): string | null {
    return this.getClaim(USER_ID_CLAIMS);
  }
  getRole(): string | null {
    return this.getClaim(ROLE_CLAIMS);
  }
  getToken(): string | null  {
    return localStorage.getItem('token') || null;
  }

  /**
   * Reads a claim from the signed token without verifying it (the API is the authority on
   * validity). Returns null when there is no readable token or the claim is absent.
   */
  private getClaim(candidates: string[]): string | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }
    try {
      const payload = this.tokenHelper.decodeToken(token);
      if (!payload) {
        return null;
      }
      for (const key of candidates) {
        const value = payload[key];
        if (value !== undefined && value !== null && value !== '') {
          return String(value);
        }
      }
    } catch {
      return null;
    }
    return null;
  }

  /**
   * True when a token is stored *and* still valid. An expired (or undecodable) token is
   * treated as no session at all and is cleared, so the UI never shows the app as
   * authenticated while the API would reject the caller with a 401. An administrator token
   * is cleared too: administrators can't use the shop.
   */
  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }
    if (this.isTokenExpired(token) || this.isAdminToken(token)) {
      this.logout();
      return false;
    }
    return true;
  }

  /** True when the token was issued to an administrator (unreadable tokens count as not). */
  private isAdminToken(token: string): boolean {
    try {
      const payload = this.tokenHelper.decodeToken(token);
      return !!payload && ROLE_CLAIMS.some(key => String(payload[key] ?? '').toLowerCase() === 'admin');
    } catch {
      return false;
    }
  }

  /** Guards against a malformed token by treating anything we cannot read as expired. */
  private isTokenExpired(token: string): boolean {
    try {
      return this.tokenHelper.isTokenExpired(token);
    } catch {
      return true;
    }
  }

  getTokenInformation(){
    if(this.isAuthenticated()){
      return this.tokenHelper.decodeToken(this.getToken() || '') || null;
    }
    return null;
  }

  registerUser(userRegister: UserRegister): Observable<boolean> {
    // Registration only returns a boolean on success (no token is issued here).
    // The register page renders the failure inline, so keep the global toastr quiet.
    return this.http.post<boolean>(this.baseApi+"/Registration", {model:userRegister}, {
      context: new HttpContext().set(SKIP_ERROR_TOAST, true),
    });
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