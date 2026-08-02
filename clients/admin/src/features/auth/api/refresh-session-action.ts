"use server";

import { cookies } from "next/headers";
import { revalidatePath } from "next/cache";
import { getSession } from "@/lib/server/session";
import { refreshSession } from "@/lib/server/refresh-session";
import { encrypt } from "@/lib/server/token-cipher";
import { isSuperAdminUser } from "@/lib/server/authorization";
import { SESSION_COOKIE_NAME, buildSessionCookieOptions } from "@/lib/server/session-cookie";

/**
 * Manually rotates the current session's tokens. Gated on super-admin here —
 * not just at the call site — since a Server Action is a callable endpoint
 * regardless of whether the triggering UI is rendered.
 */
export async function refreshSessionAction(): Promise<void> {
  const session = await getSession();
  if (!session || !isSuperAdminUser(session.profile?.userName)) return;

  const refreshed = await refreshSession(session);
  if (!refreshed) return;

  const cookieStore = await cookies();
  cookieStore.set(
    SESSION_COOKIE_NAME,
    encrypt(JSON.stringify(refreshed)),
    buildSessionCookieOptions(refreshed.sessionExpiresAt),
  );

  revalidatePath("/user-profile");
}
