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
  [ApiClients.Organization]: "ORGANIZATION_API_BASE_URL",
  [ApiClients.Approval]: "APPROVAL_API_BASE_URL",
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

/**
 * Absolute URL of the backend's SignalR notification hub. Read server-side and
 * handed to the browser at runtime (via a Server Action) rather than
 * `NEXT_PUBLIC_`-inlined, so it can be changed by editing the deployed
 * server's `.env` and restarting — no rebuild.
 */
export function getSignalRHubUrl(): string {
  return requireEnv("SIGNALR_HUB_URL");
}
