import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { parseSessionCookie } from "@/lib/server/parse-session";
import { SESSION_COOKIE_NAME } from "@/lib/server/session-cookie";

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
  const session = parseSessionCookie(request.cookies.get(SESSION_COOKIE_NAME)?.value);

  const now = Date.now();
  const sessionExpired = !session || session.sessionExpiresAt <= now;

  if (sessionExpired) {
    if (isLoginPath) return NextResponse.next();

    const response = loginRedirect(request);
    response.cookies.delete(SESSION_COOKIE_NAME);
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
