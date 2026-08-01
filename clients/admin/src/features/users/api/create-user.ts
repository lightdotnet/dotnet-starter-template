import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { CreateUserRequest } from "@/types/user";

export function createUser(accessToken: string, request: CreateUserRequest) {
  return guardCall(() =>
    requestJson<Result<string>>("user", {
      method: "POST",
      accessToken,
      body: request,
    }),
  );
}
