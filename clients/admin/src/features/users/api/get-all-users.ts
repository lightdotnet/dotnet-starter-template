import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { UserDto } from "@/types/user";

export function getAllUsers(accessToken: string) {
  return guardCall(() => requestJson<Result<UserDto[]>>("user", { accessToken }));
}
