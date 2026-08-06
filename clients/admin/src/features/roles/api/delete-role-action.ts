"use server";

import { resolveSession } from "@/features/user-profile";
import { deleteRole } from "@/features/roles/api/delete-role";

export interface DeleteRoleActionState {
  error?: string;
  success?: boolean;
}

export async function deleteRoleAction(
  id: string,
): Promise<DeleteRoleActionState> {
  const session = await resolveSession();
  if (!session) {
    return { error: "Your session has expired. Please sign in again." };
  }

  const result = await deleteRole(id);

  if (!result.isSuccess) {
    return { error: result.message || "Failed to delete role." };
  }

  return { success: true };
}
