"use server";

import { revalidatePath } from "next/cache";
import { resolveSession } from "@/features/user-profile";
import { assignEmployeeOrgUnit } from "@/features/employees/api/employees.api";
import type { AssignEmployeeOrgUnitRequest } from "@/features/employees/types/employee";

export interface AssignEmployeeOrgUnitActionState {
  error?: string;
  success?: boolean;
}

export async function assignEmployeeOrgUnitAction(
  employeeId: string,
  request: AssignEmployeeOrgUnitRequest,
): Promise<AssignEmployeeOrgUnitActionState> {
  const session = await resolveSession();
  if (!session) {
    return { error: "Your session has expired. Please sign in again." };
  }

  const result = await assignEmployeeOrgUnit(employeeId, request);

  if (!result.isSuccess) {
    return { error: result.message || "Failed to assign." };
  }

  revalidatePath("/organization/employees");
  return { success: true };
}
