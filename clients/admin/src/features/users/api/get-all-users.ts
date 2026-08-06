import { requestJson } from "@/lib/server/backend-api";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { UserDto } from "@/types/user";

export function getAllUsers() {
  return guardCall(() => requestJson<Result<UserDto[]>>("user"));
}
