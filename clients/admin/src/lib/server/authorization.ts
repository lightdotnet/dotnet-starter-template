import "server-only";

import * as shared from "@/lib/shared/authorization";
import type { SessionData } from "@/types/session";

export const SUPER_ADMIN_USERNAMES = shared.SUPER_ADMIN_USERNAMES;
export const isSuperAdminUser = shared.isSuperAdminUser;

export function hasPermission(
  session: Pick<SessionData, "permissions">,
  userName: string | null | undefined,
  permission: string,
): boolean {
  return shared.hasPermission(session.permissions, userName, permission);
}

export function hasAnyPermission(
  session: Pick<SessionData, "permissions">,
  userName: string | null | undefined,
  permissions: string[],
): boolean {
  return shared.hasAnyPermission(session.permissions, userName, permissions);
}

export function hasAllPermissions(
  session: Pick<SessionData, "permissions">,
  userName: string | null | undefined,
  permissions: string[],
): boolean {
  return shared.hasAllPermissions(session.permissions, userName, permissions);
}
