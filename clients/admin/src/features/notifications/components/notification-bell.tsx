"use client";

import { useState } from "react";
import Link from "next/link";
import { Bell } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/shared/utils";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Empty,
  EmptyDescription,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useNotifications } from "@/features/notifications/hooks/use-notifications";
import { NotificationStatus } from "@/features/notifications/types/notification";

type NotificationFilter = "all" | "unread" | "archived";

const EMPTY_MESSAGE: Record<NotificationFilter, string> = {
  all: "You're all caught up.",
  unread: "No unread notifications.",
  archived: "No archived notifications.",
};

export function NotificationBell() {
  const { notifications, unreadCount, markAsRead } = useNotifications();
  const [filter, setFilter] = useState<NotificationFilter>("all");

  const filteredNotifications = notifications.filter((notification) => {
    if (filter === "unread") return notification.status === NotificationStatus.None;
    if (filter === "archived") return notification.status === NotificationStatus.Archived;
    return true;
  });

  return (
    <DropdownMenu modal={false}>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          className="relative"
          aria-label="Notifications"
        >
          <Bell
            className={cn(
              unreadCount > 0 &&
                "origin-top [animation:bell-ring_3s_ease-in-out_infinite] motion-reduce:animate-none",
            )}
          />
          {unreadCount > 0 && (
            <Badge
              variant="destructive"
              className="absolute -top-1 -right-1 size-4 justify-center rounded-full p-0 text-[0.65rem]"
            >
              {unreadCount}
            </Badge>
          )}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-72">
        <DropdownMenuLabel>Notifications</DropdownMenuLabel>
        <DropdownMenuSeparator />

        <Tabs
          value={filter}
          onValueChange={(value) => setFilter(value as NotificationFilter)}
        >
          <TabsList className="mx-1.5 mb-1 w-full" style={{ width: "calc(100% - 0.75rem)" }}>
            <TabsTrigger value="all">All</TabsTrigger>
            <TabsTrigger value="unread">Unread</TabsTrigger>
            <TabsTrigger value="archived">Archived</TabsTrigger>
          </TabsList>
        </Tabs>

        <div className="max-h-72 overflow-y-auto">
          {filteredNotifications.length === 0 ? (
            <Empty className="p-4">
              <EmptyMedia variant="icon">
                <Bell />
              </EmptyMedia>
              <EmptyTitle>No notifications</EmptyTitle>
              <EmptyDescription>{EMPTY_MESSAGE[filter]}</EmptyDescription>
            </Empty>
          ) : (
            filteredNotifications.map((notification) => {
              const content = (
                <div className="flex w-full flex-col gap-0.5">
                  <div className="flex items-center gap-2">
                    {notification.status === NotificationStatus.None && (
                      <span
                        className="size-1.5 shrink-0 rounded-full bg-primary"
                        aria-hidden
                      />
                    )}
                    <span className="text-sm font-medium text-foreground">
                      {notification.title}
                    </span>
                  </div>
                  {notification.message && (
                    <span className="text-xs text-muted-foreground">
                      {notification.message}
                    </span>
                  )}
                  <span className="text-[0.65rem] text-muted-foreground">
                    {new Date(notification.created).toLocaleString()}
                  </span>
                </div>
              );

              return notification.url ? (
                <DropdownMenuItem key={notification.id} asChild>
                  <Link
                    href={notification.url}
                    className="flex-col items-start py-2"
                    onClick={() => void markAsRead(notification.id)}
                  >
                    {content}
                  </Link>
                </DropdownMenuItem>
              ) : (
                <DropdownMenuItem
                  key={notification.id}
                  className="flex-col items-start py-2"
                  onClick={() => void markAsRead(notification.id)}
                >
                  {content}
                </DropdownMenuItem>
              );
            })
          )}
        </div>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
