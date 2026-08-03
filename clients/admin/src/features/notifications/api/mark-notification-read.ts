import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { NotificationDto } from "@/features/notifications/types/notification";

/** `Get(entryId)` marks the entry as read as a side effect and returns the updated record. */
export function markNotificationRead(accessToken: string, id: string) {
  return guardCall(() =>
    requestJson<Result<NotificationDto | null>>(`user_notification/${id}`, { accessToken }),
  );
}
