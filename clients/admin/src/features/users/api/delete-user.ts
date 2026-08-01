import { requestJson } from "@/lib/server/http";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";

export function deleteUser(accessToken: string, id: string) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`user/${id}`, { method: "DELETE", accessToken }),
  );
}
