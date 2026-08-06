"use server";

import { resolveSession } from "@/features/user-profile";
import { markNotificationRead } from "@/features/notifications/api/mark-notification-read";

export async function markNotificationReadAction(id: string): Promise<boolean> {
  const session = await resolveSession();
  if (!session) return false;

  const result = await markNotificationRead(id);
  return result.isSuccess;
}
