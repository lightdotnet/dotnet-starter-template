"use client";

import { Bell, Menu, PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { cn } from "@/lib/shared/utils";
import { useScrolled } from "@/hooks/use-scrolled";
import { useSidebar } from "@/hooks/use-sidebar";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Breadcrumbs } from "@/components/layout/breadcrumbs";
import { Brand } from "@/components/layout/brand";
import { SearchBox } from "@/components/shared/search-box";
import { ThemeToggle, AccentColorPicker } from "@/components/theme";
import { UserMenu } from "@/components/layout/user-menu";
import type { ProfileData } from "@/types/session";

// Placeholder count — no notifications backend wired up in this UI shell.
const MOCK_NOTIFICATION_COUNT = 3;

export function TopBar({ user }: { user: ProfileData | null }) {
  const scrolled = useScrolled();
  const { hidden, toggleSidebar, setMobileOpen } = useSidebar();

  return (
    <header
      className={cn(
        "sticky top-0 z-topbar flex h-(--topbar-height) w-full shrink-0 items-center gap-3 border-b bg-sidebar px-4 text-sidebar-foreground transition-all duration-fast ease-standard",
        scrolled
          ? "border-sidebar-border bg-sidebar/70 shadow-soft-sm backdrop-blur-md"
          : "border-transparent",
      )}
    >
      {/* Mobile — opens the overlay drawer. */}
      <Button
        variant="ghost"
        size="icon"
        className="lg:hidden"
        onClick={() => setMobileOpen(true)}
        aria-label="Open navigation"
      >
        <Menu />
      </Button>

      {/* Desktop — shows/hides the sidebar entirely. */}
      <Button
        variant="ghost"
        size="icon"
        className="hidden lg:inline-flex"
        onClick={toggleSidebar}
        aria-label={hidden ? "Show sidebar" : "Hide sidebar"}
      >
        {hidden ? <PanelLeftOpen /> : <PanelLeftClose />}
      </Button>

      <Brand />

      <Breadcrumbs />

      <div className="ml-auto flex items-center gap-2">
        <SearchBox />
        <AccentColorPicker />
        <ThemeToggle />

        <Button
          variant="ghost"
          size="icon"
          className="relative"
          aria-label="Notifications"
        >
          <Bell />
          {MOCK_NOTIFICATION_COUNT > 0 && (
            <Badge
              variant="destructive"
              className="absolute -top-1 -right-1 size-4 justify-center rounded-full p-0 text-[0.65rem]"
            >
              {MOCK_NOTIFICATION_COUNT}
            </Badge>
          )}
        </Button>

        <UserMenu user={user} />
      </div>
    </header>
  );
}
