import { requestJson } from "@/lib/server/http";
import { guardRawCall } from "@/lib/server/call-guard";
import type { RoleDto } from "@/features/roles/types/role";

export function getAllRoles(accessToken: string) {
  return guardRawCall(() => requestJson<RoleDto[]>("role", { accessToken }));
}
