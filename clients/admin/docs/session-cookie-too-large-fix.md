# Fix: Login Failure When a User's Claims/Permissions Are Too Large

Reference implementation pulled from `clients/admin` in the `nexus` repo, commit `d1cad40`
("refactor(admin): consolidate session/auth into a sealed lib/server/session module").

## The bug this fixes

For a broad-permission account, the session cookie stored the same permission data **three
times** (raw JWT + a `claims[]` array + a `permissions[]` array). Once that combined, encrypted
payload passed the browser's silent **~4096-byte per-cookie limit**, the browser dropped the
cookie entirely — no error, no warning. The app was then "logged in but sessionless": every
subsequent request had no session cookie to read, which surfaced as a redirect loop and a generic
*"An unexpected response was received from the server."* error right after login.

Symptom to look for in the original project: accounts with **few** roles/permissions log in fine;
accounts with **many** (e.g. a broad admin/super-user role, or a role with dozens of granular
permission claims) fail right after login with no clear error, or bounce between the app and the
login page.

## The fix, as two independent techniques (apply both)

### 1. Stop storing derivable data — persist the minimum, derive the rest from the JWT on every read

The single biggest win: **don't put permissions/roles/claims in the cookie at all.** They already
live in the access token (JWT); decode them server-side on every read instead of persisting a
second (or third) copy.

Split the session type in two:

```ts
// types/session.ts

/**
 * The session as persisted in the encrypted cookie — deliberately minimal.
 * Anything derivable from `accessToken` (permissions, roles, claims, any
 * other JWT-sourced flag) is NOT stored here; hydration recomputes it on
 * every read. This keeps the cookie well under the browser's ~4096-byte
 * limit even for accounts with a large permission set.
 */
export interface StoredSession {
  accessToken: string;
  expiresAt: number;           // access token absolute expiry, ms since epoch
  refreshToken: string | null;
  sessionExpiresAt: number;    // absolute session expiry, ms since epoch
  profile: ProfileData | null; // display-only data, not authorization data
  // + any other field that is NOT derivable from the JWT and must be persisted
  // (e.g. a claim the JWT itself doesn't carry, sourced from a separate API)
}

/**
 * The session as the rest of the app consumes it: every StoredSession field
 * plus the authorization data hydrated from accessToken on read. Shape is
 * unchanged from before the split, so consumers need no changes.
 */
export interface SessionData extends StoredSession {
  claims: ClaimDto[];
  permissions: string[];
  roles: string[];
}
```

Hydration — decode the JWT fresh every time the session is read, never persist the result:

```ts
// lib/server/session/jwt-claims.ts
import "server-only";

/** Decodes a JWT payload without verifying signature — safe: token was just
 * issued by our own backend and is sealed inside the encrypted session
 * cookie; the backend re-validates the signature on every real API call. */
function decodeJwtClaims(token: string): Record<string, unknown> {
  const payload = token.split(".")[1];
  if (!payload) throw new Error("Invalid JWT token.");
  return JSON.parse(Buffer.from(payload, "base64url").toString("utf8"));
}

function normalizeClaimValues(value: unknown): string[] {
  if (Array.isArray(value)) return value.map(String);
  if (typeof value === "string") return [value];
  return [];
}

export function extractPermissions(accessToken: string): string[] {
  try {
    return normalizeClaimValues(decodeJwtClaims(accessToken)["permission"]);
  } catch {
    return [];
  }
}
// extractRoles(...) same shape, claim key "role"

export function extractAllClaims(accessToken: string): ClaimDto[] {
  try {
    const claims = decodeJwtClaims(accessToken);
    return Object.entries(claims).flatMap(([type, value]) =>
      normalizeClaimValues(value).map((claimValue) => ({ type, value: claimValue })),
    );
  } catch {
    return [];
  }
}
```

```ts
// lib/server/session/hydrate.ts
import "server-only";

export function hydrateSession(stored: StoredSession): SessionData {
  const claims = dedupeClaims(extractAllClaims(stored.accessToken));
  return {
    ...stored,
    permissions: extractPermissions(stored.accessToken),
    roles: extractRoles(stored.accessToken),
    claims,
  };
}
```

```ts
// lib/shared/dedupe-claims.ts — collapse exact type+value duplicates
// (a claim ending up both in the raw JWT dump and re-added explicitly, e.g. a
// claim sourced from a separate profile API merged in alongside the JWT's own).
export function dedupeClaims(claims: ClaimDto[]): ClaimDto[] {
  const seen = new Set<string>();
  const result: ClaimDto[] = [];
  for (const claim of claims) {
    const key = `${claim.type}:${claim.value}`;
    if (seen.has(key)) continue;
    seen.add(key);
    result.push(claim);
  }
  return result;
}
```

Every place that reads "the session" continues to get the same `SessionData` shape (claims,
roles, permissions all present) — this is purely an internal storage change, not a consumer-facing
one.

### 2. Belt-and-suspenders: chunk the cookie if it's still over budget

Even after removing the duplication, a single JWT can still legitimately be large (many
permission claims). Rather than trust "it'll always fit now," split the encrypted payload across
multiple numbered cookies if needed, so there's a real ceiling instead of a silent drop:

```ts
// lib/server/session/cookie-codec.ts
import "server-only";

export const SESSION_COOKIE_NAME = "admin_session";

/**
 * Max chars (== bytes; payload is ASCII base64) for a single cookie's value
 * before splitting into numbered chunk cookies. The browser rejects a cookie
 * whose whole `name=value; attrs` string exceeds ~4096 bytes, silently —
 * this leaves generous room for the name/attributes/per-cookie overhead.
 */
const MAX_CHUNK_BYTES = 3072;

/** Hard ceiling on chunk count — a guard against a pathologically large session. */
export const MAX_CHUNKS = 8;

export const ALL_SESSION_COOKIE_NAMES: readonly string[] = [
  SESSION_COOKIE_NAME,
  ...Array.from({ length: MAX_CHUNKS }, (_, i) => `${SESSION_COOKIE_NAME}.${i}`),
];

function chunkName(index: number): string {
  return `${SESSION_COOKIE_NAME}.${index}`;
}

export interface EncodedSession {
  /** `{ name, value }` pairs to `set`, in order. One entry for a small session, N for a chunked one. */
  cookies: { name: string; value: string }[];
  /** Names that must be cleared because this write doesn't use them (stale chunks, or the base name when chunking). */
  stale: string[];
  bytes: number;
}

/** Encrypts and (if needed) splits a session into the cookies that carry it.
 *  Throws — a real, loud failure — if it still can't fit within MAX_CHUNKS,
 *  instead of silently dropping data like the original bug did. */
export function encodeSession(session: StoredSession): EncodedSession {
  const payload = encrypt(JSON.stringify(session));

  if (payload.length <= MAX_CHUNK_BYTES) {
    return {
      cookies: [{ name: SESSION_COOKIE_NAME, value: payload }],
      stale: ALL_SESSION_COOKIE_NAMES.filter((n) => n !== SESSION_COOKIE_NAME),
      bytes: payload.length,
    };
  }

  const parts: string[] = [];
  for (let i = 0; i < payload.length; i += MAX_CHUNK_BYTES) {
    parts.push(payload.slice(i, i + MAX_CHUNK_BYTES));
  }
  if (parts.length > MAX_CHUNKS) {
    throw new Error(
      `Session cookie too large: ${payload.length} bytes across ${parts.length} chunks (max ${MAX_CHUNKS}).`,
    );
  }

  const used = parts.map((_, i) => chunkName(i));
  return {
    cookies: parts.map((value, i) => ({ name: chunkName(i), value })),
    stale: ALL_SESSION_COOKIE_NAMES.filter((n) => !used.includes(n)),
    bytes: payload.length,
  };
}

/** Reassembles + decrypts. Prefers the single base cookie; otherwise
 *  concatenates `<name>.0..N` until the first gap. Any tampering, a missing
 *  chunk, or a shape mismatch fails closed (returns null) rather than
 *  crashing or trusting partial data. */
export function decodeSession(read: (name: string) => string | undefined): StoredSession | null {
  const single = read(SESSION_COOKIE_NAME);
  let raw = single ?? "";

  if (!single) {
    for (let i = 0; i < MAX_CHUNKS; i++) {
      const part = read(chunkName(i));
      if (part === undefined) break;
      raw += part;
    }
  }
  if (!raw) return null;

  const plaintext = decrypt(raw);
  if (!plaintext) return null;

  try {
    const parsed: unknown = JSON.parse(plaintext);
    return isStoredSession(parsed) ? parsed : null;
  } catch {
    return null;
  }
}
```

Write path clears stale chunk cookies whenever a session shrinks back under one cookie's worth
(e.g. after switching to a smaller-permission account), so old chunks never linger:

```ts
// lib/server/session/cookie-jar.ts
import "server-only";
import { cookies } from "next/headers";

export async function writeSessionCookie(session: StoredSession): Promise<void> {
  const jar = await cookies();
  const encoded = encodeSession(session);
  const opts = cookieOptions(session.sessionExpiresAt); // httpOnly, sameSite: "lax", path: "/", maxAge from sessionExpiresAt

  for (const { name, value } of encoded.cookies) jar.set(name, value, opts);
  for (const name of encoded.stale) {
    if (jar.get(name)) jar.delete(name);
  }
}

export async function readSessionCookie(): Promise<StoredSession | null> {
  const jar = await cookies();
  return decodeSession((name) => jar.get(name)?.value);
}

export async function clearSessionCookie(): Promise<void> {
  const jar = await cookies();
  for (const name of ALL_SESSION_COOKIE_NAMES) {
    if (jar.get(name)) jar.delete(name);
  }
}
```

## Why both techniques, not just one

- **Technique 1 alone** (dedupe/derive-on-read) fixes *this* bug (3x duplication) but a future
  account with an unusually large permission set could still exceed one cookie — no ceiling, so
  the same silent-drop failure mode could resurface.
- **Technique 2 alone** (chunking) would paper over the real problem (needless duplication) and
  cost extra cookies/bytes on every request for no reason.
- Together: the common case (technique 1) keeps the cookie small and fast; the rare case
  (technique 2) still works instead of silently failing, and a genuinely pathological session
  throws a loud, diagnosable error instead of a mysterious post-login failure.

## Porting checklist

1. **Diagnose first**: confirm the original project's login failure correlates with
   accounts that have large permission/claim sets, and check whether its session
   cookie stores permissions/roles/claims as persisted fields (not just inside the raw JWT).
2. If it does, split the session type into a minimal *stored* shape (tokens, expiries, profile —
   nothing derivable from the JWT) and a *hydrated* shape (adds `claims`/`permissions`/`roles`,
   computed fresh from `accessToken` on every read). Keep the hydrated shape's fields identical to
   today's session shape so no consumer code needs to change.
3. Add a JWT-decode helper (`extractPermissions`/`extractRoles`/`extractAllClaims`) — decode-only
   (no signature verification needed; the backend already validated it and the cookie is
   encrypted/authenticated at rest), wrapped in try/catch returning `[]`/`null` on failure, never
   throwing.
4. Add `dedupeClaims` (or equivalent) wherever claims from more than one source are merged (e.g.
   JWT claims + a claim sourced from a separate profile/API call).
5. Add cookie chunking as a ceiling: encode → if payload > ~3072 bytes, split across
   `<cookie-name>.0`, `.1`, … up to a hard `MAX_CHUNKS`; throw a clear error past that ceiling
   instead of truncating/dropping. Decode: read the single cookie first, else concatenate chunks
   until the first gap; fail closed (return null) on any tamper/shape mismatch.
6. On write, always clear cookie-name slots the current write doesn't use (stale chunks from a
   previous, larger session) — otherwise a shrunk session leaves stale trailing chunks that
   corrupt the next decode.
7. Verify: log in with a normal account (confirm nothing regresses) and with the
   largest-permission account available (confirm login now succeeds and no redirect loop occurs).
