import { requestJson } from "@/lib/server/backend-api";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";

export function deleteRole(id: string) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`role/${id}`, { method: "DELETE" }),
  );
}
