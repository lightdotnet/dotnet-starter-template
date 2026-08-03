import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { PermissionDefinition } from "@/features/roles/types/permission-definition";

export function getPermissions(accessToken: string) {
  return guardCall(() =>
    requestJson<Result<PermissionDefinition[]>>("permissions", { accessToken }),
  );
}
