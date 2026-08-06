import { requestJson } from "@/lib/server/backend-api";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { CreateRoleRequest } from "@/features/roles/types/role";

export function createRole(request: CreateRoleRequest) {
  return guardCall(() =>
    requestJson<Result<string>>("role", { method: "POST", body: request }),
  );
}
