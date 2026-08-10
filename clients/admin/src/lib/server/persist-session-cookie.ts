import { cookies } from "next/headers";
import { encrypt } from "@/lib/server/token-cipher";
import {
  SESSION_COOKIE_NAME,
  buildSessionCookieOptions,
} from "@/lib/server/session-cookie";
import type { SessionData } from "@/types/session";

/**
 * Persists a session to the cookie from a Server Action / Route Handler context.
 * Not usable from middleware — `proxy.ts` sets cookies via `NextRequest`/
 * `NextResponse` instead, so it doesn't import this.
 */
export async function persistSessionCookie(session: SessionData): Promise<void> {
  const cookieStore = await cookies();
  cookieStore.set(
    SESSION_COOKIE_NAME,
    encrypt(JSON.stringify(session)),
    buildSessionCookieOptions(session.sessionExpiresAt),
  );
}
