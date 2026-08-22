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

  const outcome = await refreshSessionIfNearExpiry(session);
  const activeSession = outcome.status === "success" ? outcome.session : session;
  if (outcome.status === "success") await persistSessionCookie(activeSession);

  return { accessToken: activeSession.accessToken };
}
