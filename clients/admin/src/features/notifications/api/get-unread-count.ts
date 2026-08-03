import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";

export function getUnreadCount(accessToken: string) {
  return guardCall(() =>
    requestJson<Result<number>>("user_notification/count_unread", { accessToken }),
  );
}
