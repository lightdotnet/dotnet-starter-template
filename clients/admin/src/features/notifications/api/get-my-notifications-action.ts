"use server";

import { resolveSession } from "@/features/user-profile";
import { getMyNotifications } from "@/features/notifications/api/get-my-notifications";
import type {
  NotificationDto,
  NotificationLookupParams,
} from "@/features/notifications/types/notification";
import type { Paged } from "@/types/api";

export interface GetMyNotificationsState {
  data: Paged<NotificationDto> | null;
  error?: string;
}

export async function getMyNotificationsAction(
  params: Omit<NotificationLookupParams, "toUserId"> = {},
): Promise<GetMyNotificationsState> {
  const session = await resolveSession();
  if (!session) {
    return { data: null, error: "Your session has expired. Please sign in again." };
  }

  const result = await getMyNotifications(params);

  if (!result.isSuccess || !result.data) {
    return { data: null, error: result.message || "Failed to load notifications." };
  }

  return { data: result.data };
}
