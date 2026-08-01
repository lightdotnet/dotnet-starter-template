import { requestVoid } from "@/lib/server/http";
import { guardRawCall } from "@/lib/server/call-guard";

export function revokeSession(accessToken: string, tokenId: string) {
  return guardRawCall(() =>
    requestVoid("user_profile/token/revoke", {
      method: "PUT",
      accessToken,
      body: tokenId,
    }),
  );
}
