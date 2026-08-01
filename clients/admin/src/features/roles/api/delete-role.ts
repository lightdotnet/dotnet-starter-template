import { requestJson } from "@/lib/server/http";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";

export function deleteRole(accessToken: string, id: string) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`role/${id}`, { method: "DELETE", accessToken }),
  );
}
