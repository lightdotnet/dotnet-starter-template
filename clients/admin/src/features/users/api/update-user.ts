import { requestJson } from "@/lib/server/backend-api";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";
import type { UserDto } from "@/types/user";

export function updateUser(user: UserDto) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`user/${user.id}`, { method: "PUT", body: user }),
  );
}
