import { requestVoid } from "@/lib/server/backend-api";
import { guardRawCall } from "@/lib/server/call-guard";

export function revokeSession(tokenId: string) {
  return guardRawCall(() =>
    requestVoid("user_profile/token/revoke", { method: "PUT", body: tokenId }),
  );
}
