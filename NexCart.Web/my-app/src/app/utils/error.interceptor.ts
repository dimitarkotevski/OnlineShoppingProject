import {
  HttpContextToken,
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

/**
 * Attach this token to a request's HttpContext when the caller renders the failure
 * itself (inline validation alerts, friendly warnings, ...) so the global error
 * interceptor stays quiet for that request.
 *
 *   this.http.post(url, body, { context: new HttpContext().set(SKIP_ERROR_TOAST, true) });
 */
export const SKIP_ERROR_TOAST = new HttpContextToken<boolean>(() => false);

/**
 * Turns failed API responses into a toastr notification so the actual reason reaches
 * the user instead of a generic "something went wrong" message. The backend wraps
 * business/unhandled exceptions as { success: false, exception: "..." } and
 * validation failures as { Success: false, Errors: { field: ["..."] } }; both are
 * unwrapped here. The error is always re-thrown so callers can still react to it.
 */
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private toastr: ToastrService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (!req.context.get(SKIP_ERROR_TOAST)) {
          this.toastr.error(extractErrorMessage(error), 'Error');
        }
        return throwError(() => error);
      })
    );
  }
}

/**
 * Pulls the most useful, human-readable message out of an HTTP error. Preference
 * order: the backend's `exception` field, joined validation messages, a generic
 * `message`/`title` field, a plain string body, then a status-based fallback.
 */
function extractErrorMessage(error: HttpErrorResponse): string {
  const body: any = error.error;

  if (body && typeof body === 'object') {
    if (typeof body.exception === 'string' && body.exception.trim().length > 0) {
      return body.exception.trim();
    }

    const validationMessages = extractValidationMessages(body.Errors ?? body.errors);
    if (validationMessages.length > 0) {
      return validationMessages.join(', ');
    }

    const generic = body.message ?? body.Message ?? body.title ?? body.Title;
    if (typeof generic === 'string' && generic.trim().length > 0) {
      return generic.trim();
    }
  }

  if (typeof body === 'string' && body.trim().length > 0) {
    return body.trim();
  }

  return fallbackMessage(error);
}

/** Flattens the backend's { field: ["msg", ...] } validation envelope into plain strings. */
function extractValidationMessages(errors: any): string[] {
  if (!errors || typeof errors !== 'object') {
    return [];
  }

  const messages: string[] = [];
  for (const value of Object.values(errors)) {
    if (Array.isArray(value)) {
      for (const item of value) {
        const text = typeof item === 'string' ? item : item?.message ?? item?.Message;
        if (typeof text === 'string' && text.trim().length > 0) {
          messages.push(text.trim());
        }
      }
    } else if (typeof value === 'string' && value.trim().length > 0) {
      messages.push(value.trim());
    }
  }

  return messages;
}

/** Friendly message for failures that carry no server-provided text. */
function fallbackMessage(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return 'Unable to reach the server. Please check your connection and try again.';
  }

  switch (error.status) {
    case 400:
      return 'The request was invalid.';
    case 401:
      return 'Your session has expired. Please log in again.';
    case 403:
      return 'You are not allowed to perform this action.';
    case 404:
      return 'The requested item was not found.';
    case 429:
      return 'Too many attempts. Please wait a moment and try again.';
  }

  if (error.status >= 500) {
    return 'A server error occurred. Please try again later.';
  }

  return 'Something went wrong. Please try again.';
}
