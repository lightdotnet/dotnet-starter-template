import { getSession } from "@/lib/server/session";
import { getCurrentUser } from "@/features/user-profile/api/get-current-user";
import type { Result } from "@/types/api";
import type { SessionData } from "@/types/session";
import type { UserDto } from "@/types/user";

export interface ResolvedSession {
  session: SessionData;
  profile: Result<UserDto | null>;
}

/** Reads the session cookie and fetches the current user profile in one call. */
export async function resolveSession(): Promise<ResolvedSession | null> {
  const session = await getSession();
  if (!session) return null;

  const profile = await getCurrentUser(session.accessToken);
  return { session, profile };
}
