"use client";

import {
  createContext,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";
import { usePathname } from "next/navigation";

const HIDDEN_KEY = "admin.sidebar.hidden";
const EXPANDED_KEY = "admin.sidebar.expanded";

interface SidebarContextValue {
  hidden: boolean;
  toggleSidebar: () => void;
  isExpanded: (href: string) => boolean;
  toggleExpanded: (href: string) => void;
  mobileOpen: boolean;
  setMobileOpen: (open: boolean) => void;
}

const SidebarContext = createContext<SidebarContextValue | null>(null);

export function SidebarProvider({ children }: { children: ReactNode }) {
  const [hidden, setHidden] = useState(false);
  const [expanded, setExpanded] = useState<Set<string>>(new Set());
  const [mobileOpen, setMobileOpen] = useState(false);
  const [hydrated, setHydrated] = useState(false);

  // Close the mobile drawer after navigating — a nav click routes but Radix's
  // Sheet only closes itself on outside-click/Escape, not on in-content navigation.
  // Adjusted during render (not an effect) per the "adjusting state when a prop
  // changes" pattern: https://react.dev/learn/you-might-not-need-an-effect
  const pathname = usePathname();
  const [prevPathname, setPrevPathname] = useState(pathname);
  if (pathname !== prevPathname) {
    setPrevPathname(pathname);
    setMobileOpen(false);
  }

  // Restore persisted state after mount — avoids a server/client markup mismatch.
  // localStorage can't be read during SSR/first render, so this can't be derived inline;
  // it's a genuine one-time sync from a browser-only store, not derivable state.
  /* eslint-disable react-hooks/set-state-in-effect */
  useEffect(() => {
    const storedHidden = localStorage.getItem(HIDDEN_KEY);
    if (storedHidden !== null) setHidden(storedHidden === "true");

    const storedExpanded = localStorage.getItem(EXPANDED_KEY);
    if (storedExpanded) {
      try {
        setExpanded(new Set(JSON.parse(storedExpanded) as string[]));
      } catch {
        // ignore malformed persisted state
      }
    }
    setHydrated(true);
  }, []);
  /* eslint-enable react-hooks/set-state-in-effect */

  useEffect(() => {
    if (hydrated) localStorage.setItem(HIDDEN_KEY, String(hidden));
  }, [hidden, hydrated]);

  useEffect(() => {
    if (hydrated)
      localStorage.setItem(EXPANDED_KEY, JSON.stringify([...expanded]));
  }, [expanded, hydrated]);

  const toggleExpanded = (href: string) =>
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(href)) next.delete(href);
      else next.add(href);
      return next;
    });

  return (
    <SidebarContext.Provider
      value={{
        hidden,
        toggleSidebar: () => setHidden((prev) => !prev),
        isExpanded: (href) => expanded.has(href),
        toggleExpanded,
        mobileOpen,
        setMobileOpen,
      }}
    >
      {children}
    </SidebarContext.Provider>
  );
}

export function useSidebar() {
  const context = useContext(SidebarContext);
  if (!context) {
    throw new Error("useSidebar must be used within a SidebarProvider");
  }
  return context;
}
