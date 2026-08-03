import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { PagedResult } from "@/types/api";
import type {
  NotificationDto,
  NotificationLookupParams,
} from "@/features/notifications/types/notification";

export function getNotifications(
  accessToken: string,
  params: NotificationLookupParams = {},
) {
  return guardCall(() =>
    requestJson<PagedResult<NotificationDto>>("notification", {
      method: "GET",
      accessToken,
      query: {
        toUserId: params.toUserId,
        status: params.status !== undefined ? String(params.status) : undefined,
        pageNumber: String(params.pageNumber ?? 1),
        pageSize: String(params.pageSize ?? 10),
      },
    }),
  );
}
