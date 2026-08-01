import type { ClaimDto } from "@/types/user";

export interface SessionData {
  accessToken: string;
  expiresIn: number;
  refreshToken: string | null;
  claims: ClaimDto[];
}
