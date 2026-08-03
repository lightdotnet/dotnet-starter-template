import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { RoleDto } from "@/features/roles/types/role";

export function getAllRoles(accessToken: string) {
  return guardCall(() =>
    requestJson<Result<RoleDto[]>>("role", { accessToken }),
  );
}
