"use client";

import { useMemo } from "react";
import { Combobox } from "@/components/ui/combobox";
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
  /** Shows a clear (X) button once a user is selected. Omit for required fields. */
  onClear?: () => void;
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
  onClear,
}: UserSelectProps) {
  const options = useMemo(
    () =>
      users.map((user) => ({
        value: user.id,
        label: `${getDisplayName(user)} (@${user.userName})`,
      })),
    [users],
  );

  return (
    <Combobox
      id={id}
      name={name}
      value={value}
      onValueChange={onValueChange}
      options={options}
      placeholder={placeholder}
      className={triggerClassName}
      aria-label={ariaLabel}
      onClear={onClear}
    />
  );
}
