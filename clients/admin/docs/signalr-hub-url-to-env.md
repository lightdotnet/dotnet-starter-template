# Move `SignalRHubUrl` to a Runtime Env Var (No Rebuild Required)

Reference implementation pulled from `clients/admin` in the `nexus` repo (branch `refactor`). This
describes how the SignalR notification hub URL was moved out of the client bundle and into a
server-only environment variable, so it can be changed after `next build` by editing the deployed
`.env` and restarting — no rebuild/redeploy needed.

## Why this pattern (not `NEXT_PUBLIC_SIGNALR_HUB_URL`)

A `NEXT_PUBLIC_*` variable gets inlined into the client JS bundle **at build time**. Changing it
afterward requires a full rebuild. Since the browser needs an *absolute* backend URL to open the
SignalR connection directly (bypassing the Next.js server), the naive fix is `NEXT_PUBLIC_`, but
that reintroduces the "must rebuild to change config" problem for a value that's genuinely an
environment concern (dev/staging/prod point at different hub URLs).

The fix: keep the var **server-only** (no `NEXT_PUBLIC_` prefix), read it in a Server Action at
call time, and hand it to the browser inside the response payload — same channel already used to
hand the browser its short-lived SignalR access token, since it can't read the real session
cookie.

## The 4 pieces

### 1. Server-only config accessor

```ts
// lib/server/config.ts (or equivalent)
import "server-only";

function requireEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }
  return value;
}

/**
 * Absolute URL of the backend's SignalR notification hub. Read server-side and
 * handed to the browser at runtime (via a Server Action) rather than
 * `NEXT_PUBLIC_`-inlined, so it can be changed by editing the deployed server's
 * `.env` and restarting — no rebuild.
 */
export function getSignalRHubUrl(): string {
  return requireEnv("SIGNALR_HUB_URL");
}
```

`requireEnv` throws loudly if the var is missing — surfaces misconfiguration immediately instead
of silently connecting to `undefined`.

### 2. Server Action that hands the browser what it needs

```ts
// features/notifications/api/get-signalr-token-action.ts
"use server";

import { getFreshAccessToken } from "@/lib/server/session";
import { getSignalRHubUrl } from "@/lib/server/config";

export interface SignalRConnectionState {
  accessToken: string;
  hubUrl: string;
}

/**
 * Hands the browser what it needs to open the notification hub connection
 * directly: a short-lived access token (the JWT otherwise never leaves the
 * httpOnly session cookie) and the hub URL. The URL is resolved server-side
 * here instead of `NEXT_PUBLIC_`-inlined so it stays a runtime setting — see
 * `getSignalRHubUrl`.
 */
export async function getSignalRTokenAction(): Promise<SignalRConnectionState | null> {
  const accessToken = await getFreshAccessToken();
  if (!accessToken) return null;

  return { accessToken, hubUrl: getSignalRHubUrl() };
}
```

Bundling the token + hub URL into one Server Action call (rather than exposing the URL on its own)
means the browser never fetches config it can't yet use, and any future auth requirement on this
endpoint covers both values automatically.

If the original project doesn't already have a "get a fresh, short-lived access token server-side"
helper (`getFreshAccessToken`), that part can be dropped — the important piece for this task is
just `hubUrl: getSignalRHubUrl()`. Keep whatever auth mechanism the original project already uses
to authenticate the SignalR connection.

### 3. Client hook consumes it at connect time, not at build/module-load time

```ts
// features/notifications/hooks/use-notifications.ts (excerpt)
"use client";

import { HubConnectionBuilder, HttpTransportType } from "@microsoft/signalr";
import { getSignalRTokenAction } from "@/features/notifications/api/get-signalr-token-action";

async function connectSignalR() {
  // Both the access token and the hub URL come from the server at
  // runtime — a missing SIGNALR_HUB_URL there throws here and can be
  // handled (log + retry), so a config fix on the server is picked up
  // on the next attempt without a rebuild.
  const tokenState = await getSignalRTokenAction();
  if (!tokenState) return;

  const connection = new HubConnectionBuilder()
    .withUrl(tokenState.hubUrl, {
      accessTokenFactory: () => tokenState.accessToken,
      transport: HttpTransportType.WebSockets,
    })
    .withAutomaticReconnect()
    .build();

  await connection.start();
}
```

Key point: `tokenState.hubUrl` is read **inside** the connect function, each time it runs — never
hoisted to a module-level constant. That's what makes a `.env` edit + service restart pick up the
new value on the client's very next (re)connect attempt, with no rebuild.

Optional but recommended hardening also present in the reference implementation:
- Wrap `connection.start()` in try/catch; on failure, `setTimeout(() => connectSignalR(), 30_000)`
  to retry rather than leaving the app disconnected forever.
- Sanitize the logged error — SignalR embeds the raw negotiate response body (an HTML error page
  when the URL is misconfigured) into its thrown error message; strip anything from
  `<!DOCTYPE html`/`<html` onward before logging.

### 4. `.env` / `.env.example` entry

```dotenv
# SignalR notification hub — the browser connects directly to the backend
# (bypasses the Next.js server), so this must be an absolute URL. Requires the
# backend to allow CORS for this app's origin.
#
# Read server-side and handed to the browser at runtime by the Server Action
# above (NOT NEXT_PUBLIC_-inlined), so it is a runtime setting: change it by
# editing the deployed server's .env and restarting the service — no rebuild.
SIGNALR_HUB_URL=https://<backend-host>/signalr-hub
```

## Preconditions / things to verify in the original project before porting this

- The browser connects **directly** to the backend hub (not proxied same-origin through
  `next.config.ts` rewrites). If the original project instead proxies SignalR through the Next.js
  server, the hub URL doesn't need to reach the browser at all — this whole pattern may not apply,
  and a plain server-only fetch of the URL might be all that's needed.
- The backend must have CORS configured for the app's origin, since the hub URL is absolute and
  cross-origin from the Next.js app.
- Whatever mechanism authenticates the SignalR connection today (token in query string, header,
  `accessTokenFactory`, etc.) — keep that as-is; only the URL's origin (build-time constant →
  runtime env var read per-connect) is what this change touches.
- If the hub URL is currently `NEXT_PUBLIC_SIGNALR_HUB_URL` (build-time inlined) or a hardcoded
  string/constant, both need to be replaced with the server-only var + Server Action round trip
  above; simply renaming a `NEXT_PUBLIC_` var without moving the read to the server does **not**
  fix the "requires rebuild" problem.

## Porting checklist

1. Add `SIGNALR_HUB_URL` (server-only, no `NEXT_PUBLIC_` prefix) to `.env` / `.env.example` /
   deployment secrets.
2. Add `getSignalRHubUrl()` to the project's server-only config module, using a `requireEnv`-style
   helper (or equivalent) so a missing value fails loudly.
3. Add or extend a `"use server"` action that returns `{ hubUrl, ...whatever auth data the
   connection needs }`.
4. In the client-side SignalR connection code, call that Server Action **inside** the connect
   function (not at module scope / not memoized across reconnects) and use the returned `hubUrl` in
   `HubConnectionBuilder().withUrl(...)`.
5. Remove any existing `NEXT_PUBLIC_SIGNALR_HUB_URL` / hardcoded hub URL constant once the above is
   wired and verified.
6. Verify: change `SIGNALR_HUB_URL` in the deployed `.env`, restart the app process (no rebuild),
   confirm the client picks up the new URL on its next connection attempt.
