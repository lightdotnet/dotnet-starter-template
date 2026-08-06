import { requestJson } from "@/lib/server/backend-api";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { RoleDto } from "@/features/roles/types/role";

export function getRoleById(id: string) {
  return guardCall(() => requestJson<Result<RoleDto>>(`role/${id}`));
}
