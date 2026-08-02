import type { SessionData } from "@/types/session";

// Update this list for the new project — usernames here bypass every permission check.
export const SUPER_ADMIN_USERNAMES: string[] = ["super"];

export function isSuperAdminUser(userName: string | null | undefined): boolean {
  return !!userName && SUPER_ADMIN_USERNAMES.includes(userName);
}

export function hasPermission(
  session: Pick<SessionData, "permissions">,
  userName: string | null | undefined,
  permission: string,
): boolean {
  return isSuperAdminUser(userName) || session.permissions.includes(permission);
}

export function hasAnyPermission(
  session: Pick<SessionData, "permissions">,
  userName: string | null | undefined,
  permissions: string[],
): boolean {
  return (
    isSuperAdminUser(userName) || permissions.some((p) => session.permissions.includes(p))
  );
}

export function hasAllPermissions(
  session: Pick<SessionData, "permissions">,
  userName: string | null | undefined,
  permissions: string[],
): boolean {
  return (
    isSuperAdminUser(userName) || permissions.every((p) => session.permissions.includes(p))
  );
}
