import type { DeviceDto } from "@/features/auth";

export interface UserSessionDto {
  id: string;
  expiresAt: string | null;
  refreshTokenExpiresAt: string | null;
  device: DeviceDto | null;
}
