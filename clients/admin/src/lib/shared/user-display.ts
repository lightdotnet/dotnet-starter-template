import type { UserDto } from "@/types/user";

export function getDisplayName(user: UserDto): string {
  return user.firstName || user.lastName
    ? `${user.firstName ?? ""} ${user.lastName ?? ""}`.trim()
    : user.userName;
}

export function getInitials(user: UserDto): string {
  const initials =
    `${user.firstName?.[0] ?? ""}${user.lastName?.[0] ?? ""}`.trim();
  return initials || user.userName.slice(0, 2).toUpperCase();
}
