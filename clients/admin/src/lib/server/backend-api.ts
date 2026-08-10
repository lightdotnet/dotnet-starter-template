import "server-only";

import {
  requestJson as httpRequestJson,
  requestVoid as httpRequestVoid,
  type RequestOptions,
} from "@/lib/server/http";
import { bearerTokenHandler } from "@/lib/server/http-handlers/bearer-token-handler";
import { ApiClients } from "@/lib/server/api-clients";

type BackendRequestOptions = Omit<RequestOptions, "client">;

/**
 * Default client for the backend Modular Monolith: always targets `ApiClients.Backend`
 * and authenticates via the ambient session's access token. A service needing a
 * different auth source (e.g. a token that isn't in the session cookie yet) or a
 * different backend should call `@/lib/server/http` directly, or add a sibling
 * api-base module with its own handler instead of overriding this one per call.
 */
export function requestJson<T>(
  path: string,
  options: BackendRequestOptions = {},
): Promise<T> {
  return httpRequestJson<T>(path, {
    ...options,
    client: ApiClients.Backend,
    handlers: [bearerTokenHandler, ...(options.handlers ?? [])],
  });
}

export function requestVoid(
  path: string,
  options: BackendRequestOptions = {},
): Promise<void> {
  return httpRequestVoid(path, {
    ...options,
    client: ApiClients.Backend,
    handlers: [bearerTokenHandler, ...(options.handlers ?? [])],
  });
}
