/**
 * Client-side mirror of `NexCart.Business.Security.PasswordPolicy`.
 *
 * Keep the rules and the wording in sync with the backend so the user gets the same feedback
 * before the request is sent (and so the inline messages match what the API would return).
 */

export const PASSWORD_MIN_LENGTH = 8;
export const PASSWORD_MAX_LENGTH = 128;

export interface PasswordRequirement {
  /** Short label shown in the live checklist. */
  label: string;
  /** Returns true when the password satisfies this single requirement. */
  test: (password: string) => boolean;
}

/** Rules shown as a live checklist under the password fields. Order matters for the UI. */
export const PASSWORD_REQUIREMENTS: PasswordRequirement[] = [
  { label: `At least ${PASSWORD_MIN_LENGTH} characters`, test: (p) => p.length >= PASSWORD_MIN_LENGTH },
  { label: 'An uppercase letter (A-Z)', test: (p) => /[A-Z]/.test(p) },
  { label: 'A lowercase letter (a-z)', test: (p) => /[a-z]/.test(p) },
  { label: 'A number (0-9)', test: (p) => /[0-9]/.test(p) },
  { label: 'A special character (!@#$…)', test: (p) => /[^a-zA-Z0-9]/.test(p) },
];

/** Human-readable message for every rule the password misses (mirrors the backend). */
export function validatePassword(password: string | null | undefined): string[] {
  if (!password || !password.trim()) {
    return ['Password is required'];
  }

  const errors: string[] = [];

  if (password.length < PASSWORD_MIN_LENGTH) {
    errors.push(`Password must be at least ${PASSWORD_MIN_LENGTH} characters long`);
  }
  if (password.length > PASSWORD_MAX_LENGTH) {
    errors.push(`Password must be at most ${PASSWORD_MAX_LENGTH} characters long`);
  }
  if (!/[A-Z]/.test(password)) {
    errors.push('Password must contain at least one uppercase letter');
  }
  if (!/[a-z]/.test(password)) {
    errors.push('Password must contain at least one lowercase letter');
  }
  if (!/[0-9]/.test(password)) {
    errors.push('Password must contain at least one number');
  }
  if (!/[^a-zA-Z0-9]/.test(password)) {
    errors.push('Password must contain at least one special character');
  }

  return errors;
}

/** True when the password satisfies every requirement. */
export function isStrongPassword(password: string | null | undefined): boolean {
  return validatePassword(password).length === 0;
}
