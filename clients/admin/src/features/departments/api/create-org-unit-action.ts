"use server";

import { revalidatePath } from "next/cache";
import { resolveSession } from "@/features/user-profile";
import { createOrgUnit } from "@/features/departments/api/org-units.api";
import { OrgUnitType, type CreateOrgUnitRequest } from "@/features/departments/types/org-unit";

export interface CreateOrgUnitFormState {
  error?: string;
  success?: boolean;
}

export async function createOrgUnitAction(
  _prevState: CreateOrgUnitFormState,
  formData: FormData,
): Promise<CreateOrgUnitFormState> {
  const session = await resolveSession();
  if (!session) {
    return { error: "Your session has expired. Please sign in again." };
  }

  const companyId = String(formData.get("companyId") ?? "").trim();
  const name = String(formData.get("name") ?? "").trim();
  const code = String(formData.get("code") ?? "").trim();
  const type = String(formData.get("type") ?? OrgUnitType.Department) as OrgUnitType;

  if (!companyId || !name || !code) {
    return { error: "Name and code are required." };
  }

  const request: CreateOrgUnitRequest = {
    companyId,
    parentId: String(formData.get("parentId") ?? "") || undefined,
    type,
    name,
    code,
    description: String(formData.get("description") ?? "") || undefined,
  };

  const result = await createOrgUnit(request);

  if (!result.isSuccess) {
    return { error: result.message || "Failed to create." };
  }

  revalidatePath("/organization/departments");
  return { success: true };
}
