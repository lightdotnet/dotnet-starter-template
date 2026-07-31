"use client";

import { LogOut, Settings, User } from "lucide-react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

// Placeholder profile — this UI shell does not call src/Identity.Api yet.
const MOCK_USER = {
  firstName: "Minh",
  lastName: "Vu",
  username: "minhvu",
  email: "minhvu@example.com",
  initials: "MV",
};

export function UserMenu() {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          className="h-auto gap-2.5 rounded-lg border border-border bg-background px-5 py-1 text-foreground hover:bg-background/80"
        >
          <Avatar>
            <AvatarFallback>{MOCK_USER.initials}</AvatarFallback>
          </Avatar>
          <span className="hidden flex-col items-start gap-0 leading-none sm:flex">
            <span className="text-sm font-semibold">
              {MOCK_USER.firstName} {MOCK_USER.lastName}
            </span>
            <span className="-mt-0.5 text-xs text-muted-foreground">
              @{MOCK_USER.username}
            </span>
          </span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <DropdownMenuLabel className="flex flex-col gap-0.5 font-normal">
          <span className="text-sm font-medium text-foreground">
            {MOCK_USER.firstName} {MOCK_USER.lastName}
          </span>
          <span className="text-xs text-muted-foreground">
            {MOCK_USER.email}
          </span>
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem>
          <User />
          Profile
        </DropdownMenuItem>
        <DropdownMenuItem>
          <Settings />
          Settings
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem variant="destructive">
          <LogOut />
          Log out
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
