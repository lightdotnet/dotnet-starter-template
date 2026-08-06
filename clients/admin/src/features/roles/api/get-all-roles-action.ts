"use server";

import { resolveSession } from "@/features/user-profile";
import { getAllRoles } from "@/features/roles/api/get-all-roles";
import type { RoleDto } from "@/features/roles/types/role";

export interface GetAllRolesState {
  data: RoleDto[] | null;
  error?: string;
}

export async function getAllRolesAction(): Promise<GetAllRolesState> {
  const session = await resolveSession();
  if (!session) {
    return {
      data: null,
      error: "Your session has expired. Please sign in again.",
    };
  }

  const result = await getAllRoles();

  if (!result.isSuccess || !result.data) {
    return { data: null, error: result.message || "Failed to load roles." };
  }

  return { data: result.data };
}
