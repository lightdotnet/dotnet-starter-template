"use client";

import { Search } from "lucide-react";
import { cn } from "@/lib/shared/utils";

export function SearchBox({ className }: { className?: string }) {
  return (
    <label
      className={cn("relative hidden w-full max-w-sm sm:block", className)}
    >
      <span className="sr-only">Search</span>
      <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
      <input
        type="search"
        placeholder="Search..."
        className="h-9 w-full rounded-lg border border-input bg-background py-1.5 pr-12 pl-9 text-sm outline-none transition-colors duration-fast ease-standard placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
      />
      <kbd className="pointer-events-none absolute top-1/2 right-2.5 -translate-y-1/2 rounded-sm border border-border bg-muted px-1.5 py-0.5 text-[0.7rem] text-muted-foreground">
        ⌘K
      </kbd>
    </label>
  );
}
