import { requestJson } from "@/lib/server/http";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";
import type { RoleDto } from "@/features/roles/types/role";

export function updateRole(accessToken: string, role: RoleDto) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>("role", {
      method: "PUT",
      accessToken,
      body: role,
    }),
  );
}
