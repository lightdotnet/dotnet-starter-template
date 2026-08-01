import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { CreateRoleRequest } from "@/features/roles/types/role";

export function createRole(accessToken: string, request: CreateRoleRequest) {
  return guardCall(() =>
    requestJson<Result<string>>("role", {
      method: "POST",
      accessToken,
      body: request,
    }),
  );
}
