import "server-only";

export const ApiClients = { Backend: "Backend" } as const;

export type ApiClientName = (typeof ApiClients)[keyof typeof ApiClients];
