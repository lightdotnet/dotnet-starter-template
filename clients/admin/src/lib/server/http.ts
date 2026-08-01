import { getApiBaseUrl } from "@/lib/server/config";

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  query?: Record<string, string | undefined>;
  body?: unknown;
  accessToken?: string;
}

function buildUrl(path: string, query?: Record<string, string | undefined>): URL {
  const url = new URL(`api/v1/${path}`, getApiBaseUrl());
  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined) url.searchParams.set(key, value);
    }
  }
  return url;
}

async function send(path: string, options: RequestOptions): Promise<Response> {
  const response = await fetch(buildUrl(path, options.query), {
    method: options.method ?? "GET",
    headers: {
      "Content-Type": "application/json",
      ...(options.accessToken
        ? { Authorization: `Bearer ${options.accessToken}` }
        : {}),
    },
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  });

  if (!response.ok) {
    throw new Error(
      `Backend request to ${path} failed with status ${response.status}.`,
    );
  }

  return response;
}

export async function requestJson<T>(
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const response = await send(path, options);
  return (await response.json()) as T;
}

export async function requestVoid(
  path: string,
  options: RequestOptions = {},
): Promise<void> {
  await send(path, options);
}
