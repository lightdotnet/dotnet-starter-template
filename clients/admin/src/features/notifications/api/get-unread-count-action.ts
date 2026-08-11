"use server";

import { resolveSession } from "@/features/user-profile";
import { getUnreadCount } from "@/features/notifications/api/user-notifications.api";

export async function getUnreadCountAction(): Promise<number> {
  const session = await resolveSession();
  if (!session) return 0;

  const result = await getUnreadCount();
  if (!result.isSuccess || result.data === null) return 0;

  return result.data;
}
