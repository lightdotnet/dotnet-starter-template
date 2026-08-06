import { requestJson } from "@/lib/server/backend-api";
import { guardCall } from "@/lib/server/call-guard";
import type { Result } from "@/types/api";
import type { CreateUserRequest } from "@/types/user";

export function createUser(request: CreateUserRequest) {
  return guardCall(() =>
    requestJson<Result<string>>("user", { method: "POST", body: request }),
  );
}
