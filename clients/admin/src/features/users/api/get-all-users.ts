import { requestJson } from "@/lib/server/http";
import { guardRawCall } from "@/lib/server/call-guard";
import type { UserDto } from "@/types/user";

export function getAllUsers(accessToken: string) {
  return guardRawCall(() => requestJson<UserDto[]>("user", { accessToken }));
}
