import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { DeviceDto, GetTokenRequest, TokenDto } from "@/types/token";

export function login(request: GetTokenRequest, device?: Pick<DeviceDto, "id" | "name">) {
  // TokenController's own route ("token") plus its action route ("token/get")
  // combine into a doubled "token/token" path segment on the backend.
  return guardCall(() =>
    requestJson<Result<TokenDto>>("token/token/get", {
      method: "POST",
      body: request,
      query: {
        deviceId: device?.id ?? undefined,
        deviceName: device?.name ?? undefined,
      },
    }),
  );
}
