import { requestJson } from "@/lib/server/http";
import { guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse } from "@/types/api";

export function forcePassword(accessToken: string, id: string, password: string) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`user/${id}/password/force`, {
      method: "PUT",
      accessToken,
      body: password,
    }),
  );
}
