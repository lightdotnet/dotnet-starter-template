import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { RefreshTokenRequest, TokenDto } from "@/features/auth/types/token";

export function refreshToken(request: RefreshTokenRequest) {
  return guardCall(() =>
    requestJson<Result<TokenDto>>("token/token/refresh", {
      method: "POST",
      body: request,
    }),
  );
}
