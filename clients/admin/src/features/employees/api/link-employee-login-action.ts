"use server";

import { revalidatePath } from "next/cache";
import { resolveSession } from "@/features/user-profile";
import { linkEmployeeLogin } from "@/features/employees/api/employees.api";

export interface LinkEmployeeLoginActionState {
  error?: string;
  success?: boolean;
}

export async function linkEmployeeLoginAction(
  employeeId: string,
  userId: string,
): Promise<LinkEmployeeLoginActionState> {
  const session = await resolveSession();
  if (!session) {
    return { error: "Your session has expired. Please sign in again." };
  }

  const result = await linkEmployeeLogin(employeeId, { userId });

  if (!result.isSuccess) {
    return { error: result.message || "Failed to link login." };
  }

  revalidatePath("/organization/employees");
  return { success: true };
}
