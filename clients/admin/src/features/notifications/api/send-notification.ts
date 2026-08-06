import { requestJson } from "@/lib/server/backend-api";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";

export interface SendNotificationRequest {
  fromUserId: string;
  fromName?: string | null;
  toUserId: string;
  title: string;
  message?: string;
  url?: string;
}

export function sendNotification(request: SendNotificationRequest) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>("notification", {
      method: "POST",
      query: {
        fromUserId: request.fromUserId,
        fromName: request.fromName ?? undefined,
        toUserId: request.toUserId,
      },
      body: {
        title: request.title,
        message: request.message,
        url: request.url,
        byMessage: false,
      },
    }),
  );
}
