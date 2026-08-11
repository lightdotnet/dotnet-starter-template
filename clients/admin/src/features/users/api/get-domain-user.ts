import { requestJson } from "@/lib/server/backend-api";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { DomainUserDto } from "@/features/users/types/user";

export function getDomainUser(userName: string) {
  return guardCall(() =>
    requestJson<Result<DomainUserDto>>(`user/get_domain_user/${userName}`),
  );
}
