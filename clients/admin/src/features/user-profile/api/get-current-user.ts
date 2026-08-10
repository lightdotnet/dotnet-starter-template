import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import { explicitBearerTokenHandler } from "@/lib/server/http-handlers/bearer-token-handler";
import type { Result } from "@/types/api";
import type { UserDto } from "@/features/users";

/**
 * Takes the access token explicitly rather than reading it from the ambient
 * session — its two call sites (`login-action.ts`, `proxy.ts`) run before the
 * session cookie carrying this token has been written (or, in `proxy.ts`,
 * inside Edge middleware which never has an ambient `next/headers` session).
 */
export function getCurrentUser(accessToken: string) {
  return guardCall(() =>
    requestJson<Result<UserDto>>("user_profile", {
      handlers: [explicitBearerTokenHandler(accessToken)],
    }),
  );
}
