import { requestJson } from "@/lib/server/backend-api";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { UserDto } from "@/features/users/types/user";

export function getUserById(id: string) {
  return guardCall(() => requestJson<Result<UserDto>>(`user/${id}`));
}
