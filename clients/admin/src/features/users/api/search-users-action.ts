"use server";

import { resolveSession } from "@/features/user-profile";
import { searchUsers } from "@/features/users/api/search-users";
import type { SearchUsersParams, UserDto } from "@/types/user";
import type { Paged } from "@/types/api";

export interface SearchUsersState {
  data: Paged<UserDto> | null;
  error?: string;
}

export async function searchUsersAction(
  params: SearchUsersParams,
): Promise<SearchUsersState> {
  const session = await resolveSession();
  if (!session) {
    return {
      data: null,
      error: "Your session has expired. Please sign in again.",
    };
  }

  const result = await searchUsers(session.accessToken, params);

  if (!result.isSuccess || !result.data) {
    return { data: null, error: result.message || "Failed to search users." };
  }

  return { data: result.data };
}
