import { requestJson } from "@/lib/server/http";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";
import type { UserDto } from "@/types/user";

export function updateUser(accessToken: string, user: UserDto) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`user/${user.id}`, {
      method: "PUT",
      accessToken,
      body: user,
    }),
  );
}
