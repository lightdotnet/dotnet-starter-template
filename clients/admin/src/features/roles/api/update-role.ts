import { requestJson } from "@/lib/server/backend-api";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";
import type { RoleDto } from "@/features/roles/types/role";

export function updateRole(role: RoleDto) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>("role", { method: "PUT", body: role }),
  );
}
