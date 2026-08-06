"use server";

import { resolveSession } from "@/features/user-profile";
import { getRoleById } from "@/features/roles/api/get-role-by-id";
import type { RoleDto } from "@/features/roles/types/role";

export interface GetRoleDetailState {
  data: RoleDto | null;
  error?: string;
}

export async function getRoleDetailAction(
  id: string,
): Promise<GetRoleDetailState> {
  const session = await resolveSession();
  if (!session) {
    return {
      data: null,
      error: "Your session has expired. Please sign in again.",
    };
  }

  const result = await getRoleById(id);

  if (!result.isSuccess || !result.data) {
    return { data: null, error: result.message || "Failed to load role." };
  }

  return { data: result.data };
}
