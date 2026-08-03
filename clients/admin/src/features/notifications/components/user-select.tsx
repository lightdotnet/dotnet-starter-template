"use client";

import { useRef, useState } from "react";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { getDisplayName } from "@/lib/shared/user-display";
import type { UserDto } from "@/types/user";

interface UserSelectProps {
  id?: string;
  name?: string;
  users: UserDto[];
  value: string;
  onValueChange: (value: string) => void;
  placeholder?: string;
  triggerClassName?: string;
  ariaLabel?: string;
  /** An extra, non-user option (e.g. "All recipients" for a filter) shown above the search results, unaffected by the search text. */
  allOption?: { value: string; label: string };
}

export function UserSelect({
  id,
  name,
  users,
  value,
  onValueChange,
  placeholder = "Select a user",
  triggerClassName,
  ariaLabel,
  allOption,
}: UserSelectProps) {
  const [search, setSearch] = useState("");
  const searchInputRef = useRef<HTMLInputElement>(null);

  const filteredUsers = users.filter((user) => {
    const haystack = `${getDisplayName(user)} ${user.userName}`.toLowerCase();
    return haystack.includes(search.trim().toLowerCase());
  });

  function handleOpenChange(open: boolean) {
    if (open) {
      // Radix Select focuses the selected item on open; grab focus back for the
      // search input on the next tick instead of fighting an onOpenAutoFocus
      // prop the Select primitive (unlike Popover/Dialog) doesn't expose.
      setTimeout(() => searchInputRef.current?.focus(), 0);
    } else {
      setSearch("");
    }
  }

  return (
    <Select
      name={name}
      value={value}
      onValueChange={onValueChange}
      onOpenChange={handleOpenChange}
    >
      <SelectTrigger id={id} className={triggerClassName} aria-label={ariaLabel}>
        <SelectValue placeholder={placeholder} />
      </SelectTrigger>
      <SelectContent position="popper">
        <div className="sticky top-0 z-10 -mx-1 -mt-1 mb-1 bg-popover p-1">
          <Input
            ref={searchInputRef}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            onKeyDown={(event) => event.stopPropagation()}
            placeholder="Search user..."
            className="h-8"
          />
        </div>

        {allOption && <SelectItem value={allOption.value}>{allOption.label}</SelectItem>}

        {filteredUsers.length === 0 ? (
          <div className="px-2 py-1.5 text-sm text-muted-foreground">No users found.</div>
        ) : (
          filteredUsers.map((user) => (
            <SelectItem key={user.id} value={user.id}>
              {getDisplayName(user)} (@{user.userName})
            </SelectItem>
          ))
        )}
      </SelectContent>
    </Select>
  );
}
