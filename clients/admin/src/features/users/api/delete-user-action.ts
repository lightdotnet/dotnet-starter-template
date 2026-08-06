"use server";

import { resolveSession } from "@/features/user-profile";
import { deleteUser } from "@/features/users/api/delete-user";

export interface DeleteUserActionState {
  error?: string;
  success?: boolean;
}

export async function deleteUserAction(id: string): Promise<DeleteUserActionState> {
  const session = await resolveSession();
  if (!session) {
    return { error: "Your session has expired. Please sign in again." };
  }

  const result = await deleteUser(id);

  if (!result.isSuccess) {
    return { error: result.message || "Failed to delete user." };
  }

  return { success: true };
}
