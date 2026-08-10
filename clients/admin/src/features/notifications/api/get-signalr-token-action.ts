"use server";

import { resolveSession } from "@/features/user-profile";
import { refreshSessionIfNearExpiry } from "@/lib/server/refresh-session";
import { persistSessionCookie } from "@/lib/server/persist-session-cookie";

export interface SignalRTokenState {
  accessToken: string;
}

/**
 * Hands the browser a short-lived access token so it can authenticate the SignalR
 * handshake directly — the JWT otherwise never leaves the httpOnly session cookie.
 *
 * Proactively refreshes first if the token is near expiry, mirroring `proxy.ts` —
 * that middleware skips `/api` paths, so a long-lived session on one page (no
 * navigation) would otherwise hand SignalR a stale token and get a 401.
 */
export async function getSignalRTokenAction(): Promise<SignalRTokenState | null> {
  const session = await resolveSession();
  if (!session) return null;

  const refreshed = await refreshSessionIfNearExpiry(session);
  if (refreshed) await persistSessionCookie(refreshed);

  return { accessToken: (refreshed ?? session).accessToken };
}
