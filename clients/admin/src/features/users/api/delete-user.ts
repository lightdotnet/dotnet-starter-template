import { requestJson } from "@/lib/server/backend-api";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";

export function deleteUser(id: string) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`user/${id}`, { method: "DELETE" }),
  );
}
