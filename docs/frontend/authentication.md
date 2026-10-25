# Frontend authentication

Both SPAs use JWT bearer tokens issued by their host's `Identity` API. The token is stored in
`localStorage`; no identity data is duplicated there. The API is always the authority on
whether a token is valid.

| | Shop | Admin |
|---|---|---|
| Login endpoint | `api/Identity/Login` | `/api/Identity/AdminLogin` |
| Token key | `token` | `adminToken` |
| Token consumer | `AuthentificationService` | `core/auth.service.ts` |
| Request header | `Authorization: Bearer <token>` for `api/...` | `Authorization: Bearer <token>` for `/api/...` |
| Route protection | manual checks in pages (no guards) | `adminGuard` on the layout |
| Logout | clear token, reload `/` | clear token, navigate `/login` |

The separate storage keys matter because both sites can share a browser/origin setup: an
administrator must never appear signed in to the shop, and vice versa.

## Shop flow

1. `LoginComponent` posts `{ Username, Password }` to `api/Identity/Login`.
2. `AuthentificationService` stores the returned `Token` under `localStorage.token` and returns
   `true`/`false` to the page.
3. The page queues a flash message and does `window.location.replace('/')`; the full reload
   resets all in-memory state, and `AppComponent` shows the queued toast.
4. On every later API call, `AuthInterceptor` reads `token` and adds the bearer header when the
   request URL starts with `api/`.
5. A `401` response removes `token`. The page stays where it is; the next action requiring a
   session either redirects manually or shows the global "session expired" error.
6. Logout removes the token, emits the navbar refresh event, shows a toast and reloads `/`.

### Identity claims

`AuthentificationService` decodes (not verifies) the JWT to read:

- username — `unique_name`, `name`, or the long .NET name claim;
- user id — `nameid`, `sub`, or the long nameidentifier claim;
- role — `role` or the long role claim.

Both short and long claim names are checked because the token's outbound claim mapping decides
which form is emitted. The token is decoded with `JwtHelperService`; `isAuthenticated()` also
checks expiry and clears an expired or unreadable token.

### Shop-specific rules

- The constructor removes legacy plaintext `id`, `username` and `role` keys left by older
  builds.
- Administrator tokens are rejected: if the role claim is `admin`, the service logs out. The
  shop is for customers only.
- There are no route guards. `/cart`, `/favourite` and profile pages check the session
  themselves; other pages rely on API responses.
- The interceptor only matches URLs starting with `api/`. If you add a service with a leading
  slash (`/api/...`), the token will **not** be attached — keep the shop style, or update the
  interceptor deliberately.

## Admin flow

1. `LoginComponent` posts `{ Username, Password }` to `/api/Identity/AdminLogin`. The server
   refuses non-administrator accounts, so a normal customer cannot obtain an admin token here.
2. `AuthService.login` stores the token under `localStorage.adminToken`; the page then verifies
   `isAdmin()` before navigating to the requested (`returnUrl`) page.
3. `adminGuard` protects the layout and all admin routes. Without a valid admin token it
   redirects to `/login?returnUrl=...`; the login page accepts only same-site paths.
4. `AuthInterceptor` adds the bearer header to requests starting with `/api/` and removes the
   token on `401`.
5. Logout clears `adminToken` and navigates to `/login`.

`AuthService.isAdmin()` requires both:

- a non-expired, decodable token, and
- a role claim equal to `admin` (case-insensitive).

Any expired/unreadable token is treated as no session and cleared. When the login endpoint
returns a token without the admin role, the client logs out and shows "This account does not
have administrator access."

## Server-side expectations

The API validates every token on every request; client-side checks only drive the UI. The
backend also has an `AdminOnly` authorization policy for the admin host's controllers. Token
issuer/audience/signing key and expiry live in the API's `Jwt` settings; see
[`NexCart.Web.md`](../../NexCart.Web/NexCart.Web.md#startup-programcs) and the root
[README](../../README.md#how-the-app-works).

Expect these statuses:

- `401` — missing, expired or invalid token; the interceptors clear the stored token.
- `403` — valid token without the required role (the admin API enforces this).
- `409` — business error from login, e.g. wrong credentials (rendered by the page or toast).

## Security notes

- `localStorage` tokens are readable by JavaScript, so any XSS can steal them. Do not inject
  untrusted HTML; the safe-URL pipe exists for image bindings.
- Client-side decoding is **not** signature verification. Never trust the decoded role/id for
  authorization decisions that matter; the API enforces them.
- There is no refresh-token flow. Tokens expire (configured by the API) and the user must sign
  in again.
- Keep the shop and admin token keys and API prefixes distinct. Reusing `token` in the admin
  app or `/api` in the shop would silently break header injection or cross-app isolation.

## Adding a protected page

**Shop**

1. Read `authService.isAuthenticated()`/`getLoggedUserId()` in the page (or add a guard if the
   page should be uniformly protected).
2. Redirect to `/login` before sending requests for guests.
3. Use the existing service style (`api/...`) so `AuthInterceptor` attaches the header.
4. Decide how to handle `401`: the interceptor already clears the token and shows the global
   message; navigate to login when the page cannot continue.

**Admin**

1. Add the route as a child of the guarded `AdminLayoutComponent`.
2. Inject `AuthService` if the page needs the current admin's id/name.
3. Use `/api/...` URLs; `AuthInterceptor` handles the header.
4. Treat `403` as an authorization failure (show the global error, do not silently retry).
