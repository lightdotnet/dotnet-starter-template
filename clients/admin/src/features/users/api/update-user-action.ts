"use server";

import { resolveSession } from "@/features/user-profile";
import { updateUser } from "@/features/users/api/update-user";
import type { ClaimDto } from "@/types/claim";
import type { UserDto } from "@/features/users/types/user";

export interface UpdateUserFormState {
  error?: string;
  success?: boolean;
}

export async function updateUserAction(
  _prevState: UpdateUserFormState,
  formData: FormData,
): Promise<UpdateUserFormState> {
  const session = await resolveSession();
  if (!session) {
    return { error: "Your session has expired. Please sign in again." };
  }

  const id = String(formData.get("id") ?? "").trim();
  const userName = String(formData.get("userName") ?? "").trim();

  if (!id || !userName) {
    return { error: "Missing user identifier." };
  }

  const claimTypes = formData.getAll("claimType").map(String);
  const claimValues = formData.getAll("claimValue").map(String);
  const claims: ClaimDto[] = claimTypes
    .map((type, index) => ({ type: type.trim(), value: (claimValues[index] ?? "").trim() }))
    .filter((claim) => claim.type && claim.value);

  const user: UserDto = {
    id,
    userName,
    firstName: String(formData.get("firstName") ?? "") || undefined,
    lastName: String(formData.get("lastName") ?? "") || undefined,
    email: String(formData.get("email") ?? "") || undefined,
    phoneNumber: String(formData.get("phoneNumber") ?? "") || undefined,
    status: String(formData.get("status") ?? "") || undefined,
    authProvider: String(formData.get("authProvider") ?? "") || undefined,
    isDeleted: false,
    roles: formData.getAll("roles").map(String),
    claims,
  };

  const result = await updateUser(user);

  if (!result.isSuccess) {
    return { error: result.message || "Failed to update user." };
  }

  return { success: true };
}
