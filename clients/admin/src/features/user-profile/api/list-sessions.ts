import { requestJson } from "@/lib/server/http";
import { guardRawCall } from "@/lib/server/call-guard";
import type { UserSessionDto } from "@/features/user-profile/types/user-session";

export function listSessions(accessToken: string) {
  return guardRawCall(() =>
    requestJson<UserSessionDto[]>("user_profile/token/list", { accessToken }),
  );
}
