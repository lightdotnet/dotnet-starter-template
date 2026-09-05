"use server";

import { resolveSession } from "@/features/user-profile";
import { getEmployeeLevels } from "@/features/departments/api/employee-levels.api";
import type { EmployeeLevelDto } from "@/features/departments/types/employee-level";

export interface GetEmployeeLevelsState {
  data: EmployeeLevelDto[] | null;
  error?: string;
}

/** Used by other features (e.g. the Employees edit dialog's level picker) to self-fetch the level list for a given company on demand. */
export async function getEmployeeLevelsAction(companyId: string): Promise<GetEmployeeLevelsState> {
  const session = await resolveSession();
  if (!session) {
    return { data: null, error: "Your session has expired. Please sign in again." };
  }

  const result = await getEmployeeLevels(companyId);

  if (!result.isSuccess) {
    return { data: null, error: result.message || "Failed to load employee levels." };
  }

  return { data: result.data };
}
