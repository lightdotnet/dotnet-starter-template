import "server-only";

import { ApiClients, type ApiClientName } from "@/lib/server/api-clients";

function requireEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }
  return value;
}

const API_BASE_URL_ENV_VARS: Record<ApiClientName, string> = {
  [ApiClients.Identity]: "IDENTITY_API_BASE_URL",
  [ApiClients.Notifications]: "NOTIFICATIONS_API_BASE_URL",
};

/**
 * Callers own their full path prefix (e.g. `api/v1/`) via this base URL —
 * `buildUrl()` resolves request paths against it as-is. A trailing slash is
 * required for `URL`'s relative resolution to keep that prefix instead of
 * dropping it as a "file name" segment.
 */
export function getApiBaseUrl(client: ApiClientName): string {
  const baseUrl = requireEnv(API_BASE_URL_ENV_VARS[client]);
  return baseUrl.endsWith("/") ? baseUrl : `${baseUrl}/`;
}

export function getTokenEncryptionKey(): string {
  return requireEnv("TOKEN_ENCRYPTION_KEY");
}
