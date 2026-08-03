"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { SESSION_COOKIE_NAME } from "@/lib/server/session-cookie";

export async function logoutAction(redirectTo?: string): Promise<void> {
  const cookieStore = await cookies();
  cookieStore.delete(SESSION_COOKIE_NAME);

  // Only allow same-site relative paths — reject "//host/..." to avoid an open redirect.
  const isSafe = !!redirectTo && redirectTo.startsWith("/") && !redirectTo.startsWith("//");
  redirect(isSafe ? `/login?redirect=${encodeURIComponent(redirectTo)}` : "/login");
}
