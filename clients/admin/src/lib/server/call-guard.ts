import type { ApiResponse, Result } from "@/types/api";

function describeError(error: unknown): string {
  return error instanceof Error
    ? error.message
    : "Unexpected error while calling the API.";
}

function errorResponse(error: unknown): ApiResponse {
  return {
    requestId: "",
    code: "error",
    isSuccess: false,
    message: describeError(error),
  };
}

/**
 * Runs a backend call and normalizes any thrown error (network failure,
 * non-JSON response, timeout, ...) into the same `Result` envelope the
 * backend itself returns, so callers only ever handle one shape.
 */
export async function guardCall<T>(
  call: () => Promise<Result<T>>,
): Promise<Result<T | null>> {
  try {
    return await call();
  } catch (error) {
    return { ...errorResponse(error), data: null };
  }
}

/** Same as `guardCall`, for endpoints whose success response carries no `data`. */
export async function guardResponseCall(
  call: () => Promise<ApiResponse>,
): Promise<ApiResponse> {
  try {
    return await call();
  } catch (error) {
    return errorResponse(error);
  }
}

/** For endpoints that return a bare value/array instead of a `Result` envelope. */
export async function guardRawCall<T>(
  call: () => Promise<T>,
): Promise<
  { isSuccess: true; data: T } | { isSuccess: false; data: null; message: string }
> {
  try {
    const data = await call();
    return { isSuccess: true, data };
  } catch (error) {
    return { isSuccess: false, data: null, message: describeError(error) };
  }
}
