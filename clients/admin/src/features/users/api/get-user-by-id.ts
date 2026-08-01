import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { UserDto } from "@/types/user";

export function getUserById(accessToken: string, id: string) {
  return guardCall(() =>
    requestJson<Result<UserDto>>(`user/${id}`, { accessToken }),
  );
}
