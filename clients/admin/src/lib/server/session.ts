import { cookies } from "next/headers";
import { parseSessionCookie } from "@/lib/server/parse-session";
import { SESSION_COOKIE_NAME } from "@/lib/server/session-cookie";
import type { SessionData } from "@/types/session";

export async function getSession(): Promise<SessionData | null> {
  const cookieStore = await cookies();
  return parseSessionCookie(cookieStore.get(SESSION_COOKIE_NAME)?.value);
}
