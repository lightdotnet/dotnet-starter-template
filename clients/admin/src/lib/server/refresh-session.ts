import "server-only";

import { refreshToken } from "@/features/auth/api/token.api";
import { extractPermissions, extractRoles } from "@/lib/server/jwt";
import { REFRESH_LEAD_MS } from "@/lib/server/session-cookie";
import type { SessionData } from "@/types/session";

/**
 * Attempts to rotate the access/refresh token. Returns null on any failure —
 * callers must not treat a failed refresh as proof the session is dead:
 * multi-tab rotation races look identical to a real failure from here, and
 * the actual backend calls enforce revocation anyway.
 */
export async function refreshSession(session: SessionData): Promise<SessionData | null> {
  if (!session.refreshToken) return null;

  const result = await refreshToken({
    accessToken: session.accessToken,
    refreshToken: session.refreshToken,
  });

  if (!result.isSuccess || !result.data) return null;

  const token = result.data;
  return {
    ...session,
    accessToken: token.accessToken,
    expiresAt: Date.now() + token.expiresIn * 1000,
    refreshToken: token.refreshToken,
    // Roles/permissions live in the JWT — re-decode from the freshly-issued token.
    permissions: extractPermissions(token.accessToken),
    roles: extractRoles(token.accessToken),
  };
}

/**
 * Refreshes only if the access token is within `REFRESH_LEAD_MS` of expiring.
 * Returns null both when a refresh wasn't due and when it was attempted and
 * failed — callers only ever care whether they got a new session to persist.
 */
export async function refreshSessionIfNearExpiry(
  session: SessionData,
): Promise<SessionData | null> {
  if (session.expiresAt - Date.now() > REFRESH_LEAD_MS) return null;
  return refreshSession(session);
}
