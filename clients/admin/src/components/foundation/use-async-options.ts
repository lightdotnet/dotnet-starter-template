"use client";

import { useEffect, useRef, useState } from "react";
import type { SelectOption } from "@/components/foundation/types";

export interface UseAsyncOptionsOptions<TValue> {
  fetcher: (query: string, signal: AbortSignal) => Promise<SelectOption<TValue>[]>;
  query: string;
  enabled?: boolean;
  debounceMs?: number;
  minChars?: number;
}

export interface UseAsyncOptionsResult<TValue> {
  options: SelectOption<TValue>[];
  isLoading: boolean;
  error: string | null;
}

/** Debounced remote search with request cancellation, shared by AsyncSelect and Command Palette. */
export function useAsyncOptions<TValue>({
  fetcher,
  query,
  enabled = true,
  debounceMs = 300,
  minChars = 0,
}: UseAsyncOptionsOptions<TValue>): UseAsyncOptionsResult<TValue> {
  const [options, setOptions] = useState<SelectOption<TValue>[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetcherRef = useRef(fetcher);
  useEffect(() => {
    fetcherRef.current = fetcher;
  }, [fetcher]);

  const shouldFetch = enabled && query.trim().length >= minChars;

  useEffect(() => {
    // Nothing to fetch — no state reset here; `shouldFetch` masks the
    // previous result at the return below instead of writing state for it.
    if (!shouldFetch) return;

    const controller = new AbortController();
    setIsLoading(true);
    setError(null);

    const timeout = setTimeout(() => {
      fetcherRef
        .current(query, controller.signal)
        .then((result) => {
          if (controller.signal.aborted) return;
          setOptions(result);
          setIsLoading(false);
        })
        .catch((err: unknown) => {
          if (controller.signal.aborted) return;
          setError(err instanceof Error ? err.message : "Failed to load options.");
          setIsLoading(false);
        });
    }, debounceMs);

    return () => {
      clearTimeout(timeout);
      controller.abort();
    };
  }, [query, shouldFetch, debounceMs]);

  return {
    options: shouldFetch ? options : [],
    isLoading: shouldFetch && isLoading,
    error: shouldFetch ? error : null,
  };
}
