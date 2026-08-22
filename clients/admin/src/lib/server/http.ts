import "server-only";

import { getApiBaseUrl } from "@/lib/server/config";
import { type ApiClientName } from "@/lib/server/api-clients";

/** Thrown when a backend response is non-2xx; carries the HTTP status so callers can distinguish permanent (401/400) from transient (5xx/network) failures. */
export class HttpError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "HttpError";
  }
}

export interface HttpRequestContext {
  headers: Record<string, string>;
}

export type HttpRequestHandler = (context: HttpRequestContext) => Promise<void> | void;

export interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  query?: Record<string, string | undefined>;
  body?: unknown;
  handlers?: HttpRequestHandler[];
  client: ApiClientName;
}

function buildUrl(
  path: string,
  query: Record<string, string | undefined> | undefined,
  client: ApiClientName,
): URL {
  const url = new URL(path, getApiBaseUrl(client));
  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined) url.searchParams.set(key, value);
    }
  }
  return url;
}

/**
 * Non-2xx responses can carry a real reason in the body — this app's own
 * `ApiResponse.message` envelope, an ASP.NET `ValidationProblemDetails.errors`
 * map, or a `ProblemDetails.title` — prefer whichever is present over the
 * generic status-code message.
 */
async function extractErrorMessage(response: Response, path: string): Promise<string> {
  try {
    const body = await response.clone().json();
    if (typeof body?.message === "string" && body.message) return body.message;
    if (body?.errors && typeof body.errors === "object") {
      const firstMessage = Object.values(body.errors)
        .flat()
        .find((message) => typeof message === "string");
      if (firstMessage) return firstMessage as string;
    }
    if (typeof body?.title === "string" && body.title) return body.title;
  } catch {
    // Non-JSON body — fall through to the generic message.
  }

  return `Backend request to ${path} failed with status ${response.status}.`;
}

async function send(path: string, options: RequestOptions): Promise<Response> {
  const context: HttpRequestContext = {
    headers: { "Content-Type": "application/json" },
  };
  for (const handler of options.handlers ?? []) {
    await handler(context);
  }

  const response = await fetch(buildUrl(path, options.query, options.client), {
    method: options.method ?? "GET",
    headers: context.headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  });

  if (!response.ok) {
    throw new HttpError(response.status, await extractErrorMessage(response, path));
  }

  return response;
}

export async function requestJson<T>(
  path: string,
  options: RequestOptions,
): Promise<T> {
  const response = await send(path, options);
  return (await response.json()) as T;
}

export async function requestVoid(
  path: string,
  options: RequestOptions,
): Promise<void> {
  await send(path, options);
}
