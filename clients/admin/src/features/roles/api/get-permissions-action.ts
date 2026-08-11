"use server";

import { resolveSession } from "@/features/user-profile";
import { getPermissions } from "@/features/roles/api/roles.api";
import type { PermissionDefinition } from "@/features/roles/types/permission-definition";

export interface GetPermissionsState {
  data: PermissionDefinition[] | null;
  error?: string;
}

export async function getPermissionsAction(): Promise<GetPermissionsState> {
  const session = await resolveSession();
  if (!session) {
    return {
      data: null,
      error: "Your session has expired. Please sign in again.",
    };
  }

  const result = await getPermissions();

  if (!result.isSuccess || !result.data) {
    return {
      data: null,
      error: result.message || "Failed to load permissions.",
    };
  }

  return { data: result.data };
}
