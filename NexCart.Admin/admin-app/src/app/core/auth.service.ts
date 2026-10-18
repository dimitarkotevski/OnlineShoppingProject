import { Injectable } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';
import { SKIP_ERROR_TOAST } from './error.interceptor';

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

/**
 * localStorage key for the admin session. Deliberately different from the shop's 'token': both
 * apps share an origin, and administrators must never appear signed in to the shop.
 */
export const ADMIN_TOKEN_KEY = 'adminToken';

/** Admin-side session handling; sign-in goes through the admin-only endpoint. */
@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private baseApi = '/api/Identity';

  constructor(
    private http: HttpClient,
    private tokenHelper: JwtHelperService
  ) { }

  login(username: string, password: string): Observable<boolean> {
    // The login page shows its own message for failures, so keep the global toastr quiet.
    // AdminLogin refuses non-administrator accounts on the server.
    return this.http.post<{ Token?: string }>(this.baseApi + '/AdminLogin', { Username: username, Password: password }, {
      context: new HttpContext().set(SKIP_ERROR_TOAST, true),
    }).pipe(
      map(response => {
        if (response.Token) {
          localStorage.setItem(ADMIN_TOKEN_KEY, response.Token);
          return true;
        }
        return false;
      })
    );
  }

  logout(): void {
    localStorage.removeItem(ADMIN_TOKEN_KEY);
  }

  getToken(): string | null {
    return localStorage.getItem(ADMIN_TOKEN_KEY) || null;
  }

  getLoggedUsername(): string | undefined {
    return this.getClaim(USERNAME_CLAIMS) || undefined;
  }

  getLoggedUserId(): string | null {
    return this.getClaim(USER_ID_CLAIMS);
  }

  getRole(): string | null {
    return this.getClaim(ROLE_CLAIMS);
  }

  /**
   * True when the logged-in user holds the administrator role (case-insensitive) *and* still
   * has a valid session. The API re-checks the role on every request; this only drives the UI.
   */
  isAdmin(): boolean {
    return this.isAuthenticated() && (this.getRole() || '').toLowerCase() === 'admin';
  }

  /** True when a token is stored and not expired; an expired or unreadable token is cleared. */
  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }
    if (this.isTokenExpired(token)) {
      this.logout();
      return false;
    }
    return true;
  }

  private isTokenExpired(token: string): boolean {
    try {
      return this.tokenHelper.isTokenExpired(token);
    } catch {
      return true;
    }
  }

  /** Reads a claim from the token without verifying it (the API is the authority on validity). */
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
}
