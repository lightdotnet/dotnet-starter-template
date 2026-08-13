import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import { ApiClients } from "@/lib/server/api-clients";
import type { Result } from "@/types/api";
import type {
  DeviceDto,
  GetTokenRequest,
  RefreshTokenRequest,
  TokenDto,
} from "@/features/auth/types/token";

export function getToken(request: GetTokenRequest, device?: Pick<DeviceDto, "id" | "name">) {
  // TokenController's own route ("token") plus its action route ("token/get")
  // combine into a doubled "token/token" path segment on the backend.
  return guardCall(() =>
    requestJson<Result<TokenDto>>("auth/token/get", {
      client: ApiClients.Identity,
      method: "POST",
      body: request,
      query: {
        deviceId: device?.id ?? undefined,
        deviceName: device?.name ?? undefined,
      },
    }),
  );
}

export function refreshToken(request: RefreshTokenRequest) {
  return guardCall(() =>
    requestJson<Result<TokenDto>>("auth/token/refresh", {
      client: ApiClients.Identity,
      method: "POST",
      body: request,
    }),
  );
}
