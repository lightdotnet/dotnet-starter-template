import { requestJson } from "@/lib/server/backend-api";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";

export function forcePassword(id: string, password: string) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`user/${id}/password/force`, {
      method: "PUT",
      body: password,
    }),
  );
}
