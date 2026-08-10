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
  [ApiClients.Backend]: "API_BASE_URL",
};

export function getApiBaseUrl(client: ApiClientName = ApiClients.Backend): string {
  return requireEnv(API_BASE_URL_ENV_VARS[client]);
}

export function getTokenEncryptionKey(): string {
  return requireEnv("TOKEN_ENCRYPTION_KEY");
}
