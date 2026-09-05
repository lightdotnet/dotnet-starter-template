import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { decodeSessionCookies } from "@/lib/server/cookie-codec";
import { ALL_SESSION_COOKIE_NAMES } from "@/lib/server/session-cookie";

const LOGIN_PATH = "/login";

function loginRedirect(request: NextRequest): NextResponse {
  const loginUrl = new URL(LOGIN_PATH, request.url);
  loginUrl.searchParams.set(
    "redirect",
    `${request.nextUrl.pathname}${request.nextUrl.search}`,
  );
  return NextResponse.redirect(loginUrl);
}

/**
 * Thin auth gate only — enforces the 7-day session cap and keeps `/login`
 * unreachable once authenticated. Token refresh and profile freshness are no
 * longer handled here: they're driven client-side by `SessionGate` (a Server
 * Action + loading skeleton), since middleware blocks the whole navigation
 * with no way to show UI while it runs. See `ensure-fresh-session-action.ts`.
 */
export async function proxy(request: NextRequest) {
  const isLoginPath = request.nextUrl.pathname === LOGIN_PATH;
  const session = decodeSessionCookies((name) => request.cookies.get(name)?.value);

  const now = Date.now();
  const sessionExpired = !session || session.sessionExpiresAt <= now;

  if (sessionExpired) {
    if (isLoginPath) return NextResponse.next();

    const response = loginRedirect(request);
    for (const name of ALL_SESSION_COOKIE_NAMES) response.cookies.delete(name);
    return response;
  }

  if (isLoginPath) {
    return NextResponse.redirect(new URL("/", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|api).*)"],
};
