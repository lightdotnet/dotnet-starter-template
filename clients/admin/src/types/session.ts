import type { ClaimDto } from "@/types/claim";
import type { UserDto } from "@/features/users";

/** Display-only profile data. Excludes `roles`/`claims` — those have their own session fields. */
export type ProfileData = Omit<UserDto, "roles" | "claims">;

export interface SessionData {
  accessToken: string;
  /** Absolute expiry of `accessToken`, in ms since epoch. */
  expiresAt: number;
  refreshToken: string | null;
  /** Absolute session expiry (login time + 7 days), in ms since epoch. Not extended by refresh. */
  sessionExpiresAt: number;
  claims: ClaimDto[];
  /** Decoded from the access token's "permission" claim — never from the profile API. */
  permissions: string[];
  /** Decoded from the access token's "role" claim — never from the profile API. */
  roles: string[];
  profile: ProfileData | null;
}
