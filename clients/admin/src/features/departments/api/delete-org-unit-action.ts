"use server";

import { revalidatePath } from "next/cache";
import { resolveSession } from "@/features/user-profile";
import { deleteOrgUnit } from "@/features/departments/api/org-units.api";

export interface DeleteOrgUnitActionState {
  error?: string;
  success?: boolean;
}

export async function deleteOrgUnitAction(id: string): Promise<DeleteOrgUnitActionState> {
  const session = await resolveSession();
  if (!session) {
    return { error: "Your session has expired. Please sign in again." };
  }

  const result = await deleteOrgUnit(id);

  if (!result.isSuccess) {
    return { error: result.message || "Failed to delete." };
  }

  revalidatePath("/organization/departments");
  return { success: true };
}
