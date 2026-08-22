import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getCurrentUser } from "@/features/user-profile/api/user-profile.api";
import { encrypt } from "@/lib/server/token-cipher";
import { buildSessionClaims } from "@/lib/server/build-session-claims";
import { refreshSessionIfNearExpiry } from "@/lib/server/refresh-session";
import { parseSessionCookie } from "@/lib/server/parse-session";
import {
  MAX_REFRESH_FAILURES,
  SESSION_COOKIE_NAME,
  buildSessionCookieOptions,
} from "@/lib/server/session-cookie";
import type { ProfileData, SessionData } from "@/types/session";

const LOGIN_PATH = "/login";

function loginRedirect(request: NextRequest): NextResponse {
  const loginUrl = new URL(LOGIN_PATH, request.url);
  loginUrl.searchParams.set(
    "redirect",
    `${request.nextUrl.pathname}${request.nextUrl.search}`,
  );
  return NextResponse.redirect(loginUrl);
}

function setCookieOnRequest(request: NextRequest, session: SessionData): void {
  request.cookies.set(SESSION_COOKIE_NAME, encrypt(JSON.stringify(session)));
}

function setCookieOnResponse(response: NextResponse, session: SessionData): void {
  response.cookies.set(
    SESSION_COOKIE_NAME,
    encrypt(JSON.stringify(session)),
    buildSessionCookieOptions(session.sessionExpiresAt),
  );
}

/**
 * Refetches profile display data (name/email/status) and rebuilds `claims` as the
 * union of the access token's own claims and the profile API's raw claims — never
 * its `roles` (roles/permissions only ever come from the JWT). Returns `session`
 * unchanged on failure.
 */
async function refetchProfile(session: SessionData): Promise<SessionData> {
  const result = await getCurrentUser(session.accessToken);
  if (!result.isSuccess || !result.data) return session;

  const profileDto = result.data;
  const profile: ProfileData = {
    id: profileDto.id,
    userName: profileDto.userName,
    firstName: profileDto.firstName,
    lastName: profileDto.lastName,
    email: profileDto.email,
    phoneNumber: profileDto.phoneNumber,
    status: profileDto.status,
    authProvider: profileDto.authProvider,
    isDeleted: profileDto.isDeleted,
  };

  return {
    ...session,
    claims: buildSessionClaims(session.accessToken, profileDto.claims),
    profile,
  };
}

/**
 * A real browser navigation (typed URL, bookmark, new tab, F5) sends
 * `Sec-Fetch-Dest: document`. Next.js's own client-side route transitions use
 * `fetch()` under the hood and don't. Treat a missing header as a hard
 * navigation too (fail open toward refetching, not toward staleness).
 */
function isHardNavigation(request: NextRequest): boolean {
  const dest = request.headers.get("sec-fetch-dest");
  return dest === null || dest === "document";
}

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

  let activeSession = session;
  let tokenWasRefreshed = false;

  // A transient (network/5xx) or not-yet-due refresh is not treated as a dead session —
  // only consecutive *permanent* (401/400) failures count, see `refreshSession` doc comment.
  const outcome = await refreshSessionIfNearExpiry(session);
  if (outcome.status === "success") {
    activeSession = outcome.session;
    tokenWasRefreshed = true;
  } else if (outcome.status === "failed" && outcome.permanent) {
    const failureCount = (session.refreshFailureCount ?? 0) + 1;
    if (failureCount >= MAX_REFRESH_FAILURES) {
      const response = loginRedirect(request);
      response.cookies.delete(SESSION_COOKIE_NAME);
      return response;
    }
    activeSession = { ...session, refreshFailureCount: failureCount };
  }

  // Profile display data (name/email/claims) is only refetched on a real page
  // load or right after a token refresh — not on every in-app navigation.
  if (isHardNavigation(request) || tokenWasRefreshed) {
    activeSession = await refetchProfile(activeSession);
  }

  if (activeSession !== session) {
    setCookieOnRequest(request, activeSession);
  }

  const response = NextResponse.next({ request: { headers: request.headers } });

  if (activeSession !== session) {
    setCookieOnResponse(response, activeSession);
  }

  return response;
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|api).*)"],
};
