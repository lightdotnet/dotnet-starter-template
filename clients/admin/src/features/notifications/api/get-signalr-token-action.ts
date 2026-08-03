"use server";

import { resolveSession } from "@/features/user-profile";

export interface SignalRTokenState {
  accessToken: string;
}

/** Hands the browser a short-lived access token so it can authenticate the SignalR handshake directly — the JWT otherwise never leaves the httpOnly session cookie. */
export async function getSignalRTokenAction(): Promise<SignalRTokenState | null> {
  const session = await resolveSession();
  if (!session) return null;

  return { accessToken: session.accessToken };
}
