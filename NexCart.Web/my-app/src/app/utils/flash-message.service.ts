import { Injectable } from '@angular/core';

export type FlashMessageType = 'success' | 'error' | 'info' | 'warning';

export interface FlashMessage {
  message: string;
  type: FlashMessageType;
}

/**
 * Carries a one-shot toastr message across a full page reload.
 *
 * The login and registration flows finish with `window.location.replace(...)`, which
 * tears down the whole Angular app (and any toast that was already on screen) before
 * it can be seen. The message is parked in sessionStorage instead and is shown by the
 * app root the first time it boots after the redirect.
 *
 *   this.flashMessage.set('User successfully created!');
 *   window.location.replace('/login');
 */
@Injectable({ providedIn: 'root' })
export class FlashMessageService {
  private static readonly STORAGE_KEY = 'flashMessage';

  set(message: string, type: FlashMessageType = 'success'): void {
    try {
      sessionStorage.setItem(
        FlashMessageService.STORAGE_KEY,
        JSON.stringify({ message, type } satisfies FlashMessage)
      );
    } catch {
      // Storage may be unavailable (quota/private mode); losing the toast is acceptable.
    }
  }

  /** Reads and clears the pending message so it can only ever be shown once. */
  consume(): FlashMessage | null {
    try {
      const raw = sessionStorage.getItem(FlashMessageService.STORAGE_KEY);
      if (!raw) {
        return null;
      }
      sessionStorage.removeItem(FlashMessageService.STORAGE_KEY);
      return JSON.parse(raw) as FlashMessage;
    } catch {
      return null;
    }
  }
}
