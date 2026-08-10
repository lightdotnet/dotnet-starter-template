import "server-only";

import { decrypt } from "@/lib/server/token-cipher";
import type { SessionData } from "@/types/session";

function isSessionData(value: unknown): value is SessionData {
  if (typeof value !== "object" || value === null) return false;
  const v = value as Record<string, unknown>;

  return (
    typeof v.accessToken === "string" &&
    typeof v.expiresAt === "number" &&
    typeof v.sessionExpiresAt === "number" &&
    Array.isArray(v.claims) &&
    Array.isArray(v.permissions) &&
    Array.isArray(v.roles)
  );
}

/** Decrypts and validates a raw session cookie value. Returns null for anything missing/malformed. */
export function parseSessionCookie(raw: string | undefined): SessionData | null {
  if (!raw) return null;

  const plaintext = decrypt(raw);
  if (!plaintext) return null;

  try {
    const parsed: unknown = JSON.parse(plaintext);
    return isSessionData(parsed) ? parsed : null;
  } catch {
    return null;
  }
}
