"use client";

import { Check, Palette } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { ACCENT_COLORS, useAccentColor } from "@/hooks/use-accent-color";

export function AccentColorPicker() {
  const { accentColor, setAccentColor } = useAccentColor();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" aria-label="Change accent color">
          <Palette />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuLabel>Accent color</DropdownMenuLabel>
        <DropdownMenuSeparator />
        {ACCENT_COLORS.map(({ value, label }) => (
          <DropdownMenuItem key={value} onSelect={() => setAccentColor(value)}>
            <span
              className="size-3.5 shrink-0 rounded-full"
              style={{ backgroundColor: `var(--color-${value}-600)` }}
              aria-hidden="true"
            />
            {label}
            {accentColor === value && <Check className="ml-auto size-4" />}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
