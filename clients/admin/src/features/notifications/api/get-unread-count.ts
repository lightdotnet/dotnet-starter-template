import { requestJson } from "@/lib/server/backend-api";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";

export function getUnreadCount() {
  return guardCall(() =>
    requestJson<Result<number>>("user_notification/count_unread"),
  );
}
