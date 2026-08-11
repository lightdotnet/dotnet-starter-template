"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { getToken } from "@/features/auth/api/token.api";
import { getCurrentUser } from "@/features/user-profile";
import { encrypt } from "@/lib/server/token-cipher";
import { extractPermissions, extractRoles } from "@/lib/server/jwt";
import { buildSessionClaims } from "@/lib/server/build-session-claims";
import {
  SESSION_COOKIE_NAME,
  SESSION_TTL_MS,
  buildSessionCookieOptions,
} from "@/lib/server/session-cookie";
import type { ProfileData, SessionData } from "@/types/session";

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

  const tokenResult = await getToken({ username, password });

  if (!tokenResult.isSuccess || !tokenResult.data) {
    return { error: tokenResult.message || "Login failed." };
  }

  const token = tokenResult.data;

  // Profile/claims fetch failure shouldn't block a successful login — just
  // start the session with no claims/profile rather than failing the whole sign-in.
  const profileResult = await getCurrentUser(token.accessToken);
  const profileDto =
    profileResult.isSuccess && profileResult.data ? profileResult.data : null;

  // JWT claims are always available regardless of whether the profile fetch succeeded.
  const claims = buildSessionClaims(token.accessToken, profileDto?.claims ?? []);
  const profile: ProfileData | null = profileDto
    ? {
        id: profileDto.id,
        userName: profileDto.userName,
        firstName: profileDto.firstName,
        lastName: profileDto.lastName,
        email: profileDto.email,
        phoneNumber: profileDto.phoneNumber,
        status: profileDto.status,
        authProvider: profileDto.authProvider,
        isDeleted: profileDto.isDeleted,
      }
    : null;

  const now = Date.now();
  const sessionExpiresAt = now + SESSION_TTL_MS;

  const session: SessionData = {
    accessToken: token.accessToken,
    expiresAt: now + token.expiresIn * 1000,
    refreshToken: token.refreshToken,
    sessionExpiresAt,
    claims,
    // Roles/permissions come from the JWT only — never from the profile API.
    permissions: extractPermissions(token.accessToken),
    roles: extractRoles(token.accessToken),
    profile,
  };

  const cookieStore = await cookies();
  cookieStore.set(
    SESSION_COOKIE_NAME,
    encrypt(JSON.stringify(session)),
    buildSessionCookieOptions(sessionExpiresAt),
  );

  const redirectTo = String(formData.get("redirect") ?? "");
  // Only allow same-site relative paths — reject "//host/..." to avoid an open redirect.
  const destination =
    redirectTo.startsWith("/") && !redirectTo.startsWith("//") ? redirectTo : "/";

  redirect(destination);
}
