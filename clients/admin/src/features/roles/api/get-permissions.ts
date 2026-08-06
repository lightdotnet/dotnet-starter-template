import { requestJson } from "@/lib/server/backend-api";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { PermissionDefinition } from "@/features/roles/types/permission-definition";

export function getPermissions() {
  return guardCall(() =>
    requestJson<Result<PermissionDefinition[]>>("permissions"),
  );
}
