import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ADMIN_TOKEN_KEY } from './auth.service';

/**
 * Attaches the stored JWT to API requests and clears the session when the token
 * is rejected, so the (now authenticated) backend can identify the caller.
 */
@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem(ADMIN_TOKEN_KEY);

    let authReq = req;
    if (token && req.url.toLowerCase().startsWith('/api/')) {
      authReq = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    }

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // The session is gone; drop the token. Identity is derived from it, so nothing else to clear.
          localStorage.removeItem(ADMIN_TOKEN_KEY);
        }
        return throwError(() => error);
      })
    );
  }
}
