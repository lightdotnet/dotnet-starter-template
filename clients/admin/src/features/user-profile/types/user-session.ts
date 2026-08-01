import type { DeviceDto } from "@/types/token";

export interface UserSessionDto {
  id: string;
  expiresAt: string | null;
  refreshTokenExpiresAt: string | null;
  device: DeviceDto | null;
}
