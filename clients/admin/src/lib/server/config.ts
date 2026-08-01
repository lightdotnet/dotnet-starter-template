function requireEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }
  return value;
}

export function getApiBaseUrl(): string {
  return requireEnv("API_BASE_URL");
}

export function getTokenEncryptionKey(): string {
  return requireEnv("TOKEN_ENCRYPTION_KEY");
}
