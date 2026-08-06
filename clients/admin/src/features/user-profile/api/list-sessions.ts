import { requestJson } from "@/lib/server/backend-api";
import { guardRawCall } from "@/lib/server/call-guard";
import type { UserSessionDto } from "@/features/user-profile/types/user-session";

export function listSessions() {
  return guardRawCall(() =>
    requestJson<UserSessionDto[]>("user_profile/token/list"),
  );
}
