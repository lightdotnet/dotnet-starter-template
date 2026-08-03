"use server";

import { resolveSession } from "@/features/user-profile";
import { getUserById } from "@/features/users/api/get-user-by-id";
import type { UserDto } from "@/types/user";

export interface GetUserDetailState {
  data: UserDto | null;
  error?: string;
}

export async function getUserDetailAction(id: string): Promise<GetUserDetailState> {
  const session = await resolveSession();
  if (!session) {
    return { data: null, error: "Your session has expired. Please sign in again." };
  }

  const result = await getUserById(session.accessToken, id);

  if (!result.isSuccess || !result.data) {
    return { data: null, error: result.message || "Failed to load user." };
  }

  return { data: result.data };
}
