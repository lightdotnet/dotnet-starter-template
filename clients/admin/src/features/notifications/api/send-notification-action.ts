"use server";

import { resolveSession } from "@/features/user-profile";
import { sendNotification } from "@/features/notifications/api/send-notification";
import { getDisplayName } from "@/lib/shared/user-display";

export interface SendNotificationFormState {
  error?: string;
  success?: boolean;
}

export async function sendNotificationAction(
  _prevState: SendNotificationFormState,
  formData: FormData,
): Promise<SendNotificationFormState> {
  const session = await resolveSession();
  if (!session || !session.profile) {
    return { error: "Your session has expired. Please sign in again." };
  }

  const toUserId = String(formData.get("toUserId") ?? "").trim();
  const title = String(formData.get("title") ?? "").trim();

  if (!toUserId || !title) {
    return { error: "Recipient and title are required." };
  }

  const result = await sendNotification(session.accessToken, {
    fromUserId: session.profile.id,
    fromName: getDisplayName(session.profile),
    toUserId,
    title,
    message: String(formData.get("message") ?? "") || undefined,
    url: String(formData.get("url") ?? "") || undefined,
  });

  if (!result.isSuccess) {
    return { error: result.message || "Failed to send notification." };
  }

  return { success: true };
}
