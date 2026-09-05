import "server-only";

export const ApiClients = {
  Identity: "Identity",
  Notifications: "Notifications",
  Organization: "Organization",
} as const;

export type ApiClientName = (typeof ApiClients)[keyof typeof ApiClients];
