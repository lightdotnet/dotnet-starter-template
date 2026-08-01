"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { login } from "@/features/auth/api/login";
import { getCurrentUser } from "@/features/user-profile";
import { dedupeClaims } from "@/lib/shared/dedupe-claims";
import { SESSION_COOKIE_NAME } from "@/lib/server/session-cookie";
import type { SessionData } from "@/types/session";

export interface LoginFormState {
  error?: string;
}

export async function loginAction(
  _prevState: LoginFormState,
  formData: FormData,
): Promise<LoginFormState> {
  const username = String(formData.get("username") ?? "");
  const password = String(formData.get("password") ?? "");

  if (!username || !password) {
    return { error: "Username and password are required." };
  }

  const tokenResult = await login({ username, password });

  if (!tokenResult.isSuccess || !tokenResult.data) {
    return { error: tokenResult.message || "Login failed." };
  }

  const token = tokenResult.data;

  // Profile/claims fetch failure shouldn't block a successful login — just
  // start the session with no claims rather than failing the whole sign-in.
  const profileResult = await getCurrentUser(token.accessToken);
  const claims =
    profileResult.isSuccess && profileResult.data
      ? dedupeClaims(profileResult.data.claims)
      : [];

  const session: SessionData = {
    accessToken: token.accessToken,
    expiresIn: token.expiresIn,
    refreshToken: token.refreshToken,
    claims,
  };

  const cookieStore = await cookies();
  // Plaintext for now — encryption is a separate follow-up step.
  cookieStore.set(SESSION_COOKIE_NAME, JSON.stringify(session), {
    httpOnly: true,
    sameSite: "lax",
    path: "/",
    maxAge: token.expiresIn,
  });

  const from = String(formData.get("from") ?? "");
  // Only allow same-site relative paths — reject "//host/..." to avoid an open redirect.
  const destination = from.startsWith("/") && !from.startsWith("//") ? from : "/";

  redirect(destination);
}
