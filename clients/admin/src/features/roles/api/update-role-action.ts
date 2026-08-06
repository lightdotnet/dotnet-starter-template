"use server";

import { resolveSession } from "@/features/user-profile";
import { getRoleById } from "@/features/roles/api/get-role-by-id";
import { updateRole } from "@/features/roles/api/update-role";
import type { RoleDto } from "@/features/roles/types/role";

export interface UpdateRoleFormState {
  error?: string;
  success?: boolean;
}

const PERMISSION_CLAIM_TYPE = "permission";

export async function updateRoleAction(
  _prevState: UpdateRoleFormState,
  formData: FormData,
): Promise<UpdateRoleFormState> {
  const session = await resolveSession();
  if (!session) {
    return { error: "Your session has expired. Please sign in again." };
  }

  const id = String(formData.get("id") ?? "").trim();
  const name = String(formData.get("name") ?? "").trim();

  if (!id || !name) {
    return { error: "Name is required." };
  }

  const current = await getRoleById(id);
  if (!current.isSuccess || !current.data) {
    return { error: current.message || "Failed to load the current role." };
  }

  const nonPermissionClaims = current.data.claims.filter(
    (claim) => claim.type !== PERMISSION_CLAIM_TYPE,
  );
  const permissionClaims = formData
    .getAll("permissions")
    .map((value) => ({ type: PERMISSION_CLAIM_TYPE, value: String(value) }));

  const role: RoleDto = {
    id,
    name,
    description: String(formData.get("description") ?? "") || undefined,
    claims: [...nonPermissionClaims, ...permissionClaims],
  };

  const result = await updateRole(role);

  if (!result.isSuccess) {
    return { error: result.message || "Failed to update role." };
  }

  return { success: true };
}
