"use client";

import { useId, useMemo, useState } from "react";
import { ChevronDown, X } from "lucide-react";
import { FloatingPortal, useInteractions } from "@floating-ui/react";
import { cn } from "@/lib/shared/utils";
import { useFloatingPopover } from "@/components/foundation/use-floating-popover";
import { useListbox } from "@/components/foundation/use-listbox";
import { OptionsList } from "@/components/foundation/options-list";
import type { RenderOption, SelectOption } from "@/components/foundation/types";

export interface EntitySelectProps<TValue extends string = string> {
  options: SelectOption<TValue>[];
  value: TValue | null;
  onValueChange: (value: TValue) => void;
  placeholder?: string;
  disabled?: boolean;
  id?: string;
  /** Renders a hidden input so this select can submit inside a native `<form>`. */
  name?: string;
  "aria-label"?: string;
  renderOption?: RenderOption<TValue>;
  className?: string;
  /** Shows a clear (X) button in place of the chevron once a value is selected. Omit for required fields. */
  onClear?: () => void;
}

/**
 * Single-select bound to an already-fetched option list — the direct
 * replacement for the Radix `Select` for non-searchable cases. No text
 * input: keyboard nav moves an `aria-activedescendant` while real focus
 * stays on the trigger button (WAI-ARIA "Select-Only" listbox pattern).
 */
export function EntitySelect<TValue extends string = string>({
  options,
  value,
  onValueChange,
  placeholder = "Select...",
  disabled,
  id,
  name,
  "aria-label": ariaLabel,
  renderOption,
  className,
  onClear,
}: EntitySelectProps<TValue>) {
  const listboxId = useId();
  const [activeIndex, setActiveIndex] = useState<number | null>(null);

  const selectedIndex = useMemo(() => {
    const index = options.findIndex((option) => option.value === value);
    return index === -1 ? null : index;
  }, [options, value]);

  const popover = useFloatingPopover({ placement: "bottom-start", role: "listbox" });
  const { listRef, listNavigation, typeahead } = useListbox({
    context: popover.context,
    labels: options.map((option) => option.label),
    activeIndex,
    onActiveIndexChange: setActiveIndex,
    selectedIndex,
  });

  const { getReferenceProps, getFloatingProps, getItemProps } = useInteractions([
    popover.click,
    popover.dismiss,
    popover.role,
    listNavigation,
    typeahead,
  ]);

  const selectedOption = selectedIndex !== null ? options[selectedIndex] : undefined;

  function handleSelect(option: SelectOption<TValue>) {
    onValueChange(option.value);
    popover.setOpen(false);
  }

  const showClear = !!onClear && !!selectedOption;

  return (
    <>
      {name && <input type="hidden" name={name} value={value ?? ""} />}
      <div className="relative inline-flex">
        <button
          type="button"
          {...getReferenceProps()}
          id={id}
          disabled={disabled}
          ref={popover.refs.setReference}
          // WAI-ARIA APG "select-only combobox" pattern: role="combobox" (not
          // the button's implicit "button" role) is what makes
          // aria-activedescendant valid here.
          role="combobox"
          aria-label={ariaLabel}
          aria-expanded={popover.open}
          aria-activedescendant={
            popover.open && activeIndex !== null ? `${listboxId}-option-${activeIndex}` : undefined
          }
          data-placeholder={!selectedOption || undefined}
          className={cn(
            "flex h-8 w-fit items-center justify-between gap-1.5 rounded-md border border-input bg-transparent py-2 pr-2 pl-2.5 text-sm whitespace-nowrap transition-colors outline-none select-none focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/50 disabled:cursor-not-allowed disabled:opacity-50 data-placeholder:text-muted-foreground dark:bg-input/30 dark:hover:bg-input/50",
            className,
          )}
        >
          <span className="line-clamp-1 flex items-center gap-1.5">
            {selectedOption ? selectedOption.label : placeholder}
          </span>
          <ChevronDown
            className={cn(
              "pointer-events-none size-4 shrink-0 text-muted-foreground",
              showClear && "invisible",
            )}
          />
        </button>
        {showClear && (
          <button
            type="button"
            aria-label="Clear selection"
            onClick={(event) => {
              event.stopPropagation();
              popover.setOpen(false);
              onClear();
            }}
            className="absolute top-1/2 right-2 flex size-4 -translate-y-1/2 items-center justify-center rounded-sm text-muted-foreground hover:text-foreground"
          >
            <X className="size-3.5" />
          </button>
        )}
      </div>

      {popover.open && (
        <FloatingPortal>
          <div
            ref={popover.refs.setFloating}
            style={popover.floatingStyles}
            className="z-50 min-w-36 overflow-hidden rounded-lg bg-popover text-popover-foreground shadow-md ring-1 ring-foreground/10"
            {...getFloatingProps()}
          >
            <OptionsList
              id={listboxId}
              options={options}
              listRef={listRef}
              getItemProps={getItemProps}
              activeIndex={activeIndex}
              isSelected={(option) => option.value === value}
              onSelect={handleSelect}
              renderOption={renderOption}
            />
          </div>
        </FloatingPortal>
      )}
    </>
  );
}
