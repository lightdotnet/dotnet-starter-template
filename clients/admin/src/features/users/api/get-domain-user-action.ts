"use server";

import { resolveSession } from "@/features/user-profile";
import { getDomainUser } from "@/features/users/api/get-domain-user";
import type { DomainUserDto } from "@/features/users/types/user";

export interface GetDomainUserState {
  data: DomainUserDto | null;
  error?: string;
}

export async function getDomainUserAction(userName: string): Promise<GetDomainUserState> {
  const session = await resolveSession();
  if (!session) {
    return { data: null, error: "Your session has expired. Please sign in again." };
  }

  const result = await getDomainUser(userName);

  if (!result.isSuccess || !result.data) {
    return { data: null, error: result.message || "Domain user not found." };
  }

  return { data: result.data };
}
