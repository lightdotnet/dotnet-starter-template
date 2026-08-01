import { cookies } from "next/headers";
import { SESSION_COOKIE_NAME } from "@/lib/server/session-cookie";
import type { SessionData } from "@/types/session";

function parseSession(raw: string | undefined): SessionData | null {
  if (!raw) return null;
  try {
    return JSON.parse(raw) as SessionData;
  } catch {
    return null;
  }
}

export async function getSession(): Promise<SessionData | null> {
  const cookieStore = await cookies();
  return parseSession(cookieStore.get(SESSION_COOKIE_NAME)?.value);
}
