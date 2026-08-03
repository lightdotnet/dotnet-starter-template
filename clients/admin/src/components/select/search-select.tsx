"use client";

import { useDeferredValue, useId, useMemo, useState, type ChangeEvent, type KeyboardEvent } from "react";
import { ChevronDownIcon, SearchIcon, XIcon } from "lucide-react";
import { FloatingPortal, useInteractions } from "@floating-ui/react";
import { cn } from "@/lib/shared/utils";
import { useFloatingPopover } from "@/components/foundation/use-floating-popover";
import { useListbox } from "@/components/foundation/use-listbox";
import { OptionsList } from "@/components/foundation/options-list";
import type { RenderOption, SelectOption } from "@/components/foundation/types";

export interface SearchSelectProps<TValue extends string = string> {
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
  /** Defaults to a case-insensitive label substring match. */
  filter?: (option: SelectOption<TValue>, query: string) => boolean;
  className?: string;
  /** Shows a clear (X) button in place of the chevron once a value is selected. Omit for required fields. */
  onClear?: () => void;
  /** Minimum characters typed before the list narrows to matches; below this, the full option list still shows. Defaults to 3. */
  minChars?: number;
}

const DEFAULT_MIN_CHARS = 3;

function defaultFilter<TValue>(option: SelectOption<TValue>, query: string) {
  return option.label.toLowerCase().includes(query.trim().toLowerCase());
}

/**
 * Client-side filterable combobox — the WAI-ARIA APG "editable combobox with
 * list autocomplete" pattern. The correct replacement for the
 * input-stuffed-inside-Select hack previously used for recipient pickers.
 */
export function SearchSelect<TValue extends string = string>({
  options,
  value,
  onValueChange,
  placeholder = "Search...",
  disabled,
  id,
  name,
  "aria-label": ariaLabel,
  renderOption,
  filter = defaultFilter,
  className,
  onClear,
  minChars = DEFAULT_MIN_CHARS,
}: SearchSelectProps<TValue>) {
  const listboxId = useId();
  const [activeIndex, setActiveIndex] = useState<number | null>(null);
  const [query, setQuery] = useState<string | null>(null);
  const deferredQuery = useDeferredValue(query ?? "");

  const selectedOption = useMemo(
    () => options.find((option) => option.value === value),
    [options, value],
  );

  // `query === null` means "not actively editing" — show the selected
  // option's label. Once the user types, `query` takes over until blur/select.
  const inputText = query ?? selectedOption?.label ?? "";

  // Below `minChars`, keep showing the full list rather than a narrowed
  // one — there's no backend cost to this (options are already in memory),
  // it just avoids over-eagerly filtering on a 1-2 character query.
  const filteredOptions = useMemo(
    () =>
      query === null || query.trim().length < minChars
        ? options
        : options.filter((option) => filter(option, deferredQuery)),
    [options, query, deferredQuery, filter, minChars],
  );

  // enableClick: false — this trigger is a text input, not a button. It
  // already opens via onFocus below; leaving useClick's toggle enabled would
  // make the click that follows that focus event immediately close it again.
  const popover = useFloatingPopover({
    placement: "bottom-start",
    role: "listbox",
    enableClick: false,
  });
  // enableTypeahead: false — this is a real text input; the query already
  // filters as-you-type, and useTypeahead would otherwise preventDefault
  // every character keydown while open, blocking typing entirely.
  const { listRef, listNavigation, typeahead } = useListbox({
    context: popover.context,
    labels: filteredOptions.map((option) => option.label),
    activeIndex,
    onActiveIndexChange: setActiveIndex,
    selectedIndex: null,
    enableTypeahead: false,
  });

  const { getReferenceProps, getFloatingProps, getItemProps } = useInteractions([
    popover.click,
    popover.dismiss,
    popover.role,
    listNavigation,
    typeahead,
  ]);

  function handleSelect(option: SelectOption<TValue>) {
    onValueChange(option.value);
    setQuery(null);
    setActiveIndex(null);
    popover.setOpen(false);
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") {
      event.preventDefault();
      if (activeIndex !== null) {
        const option = filteredOptions[activeIndex];
        if (option && !option.disabled) handleSelect(option);
      }
    } else if (event.key === "Escape") {
      setQuery(null);
      setActiveIndex(null);
    }
  }

  const showClear = !!onClear && !!selectedOption;

  return (
    <>
      {name && <input type="hidden" name={name} value={value ?? ""} />}
      <div className="relative">
        <SearchIcon className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
        <input
          {...getReferenceProps({
            onChange: (event: ChangeEvent<HTMLInputElement>) => {
              setQuery(event.target.value);
              setActiveIndex(null);
              popover.setOpen(true);
            },
            onFocus: () => popover.setOpen(true),
            onClick: () => popover.setOpen(true),
            onBlur: () => {
              setQuery(null);
              popover.setOpen(false);
            },
            onKeyDown: handleKeyDown,
          })}
          id={id}
          ref={popover.refs.setReference}
          disabled={disabled}
          role="combobox"
          aria-label={ariaLabel}
          aria-autocomplete="list"
          aria-expanded={popover.open}
          aria-controls={listboxId}
          aria-activedescendant={
            popover.open && activeIndex !== null ? `${listboxId}-option-${activeIndex}` : undefined
          }
          value={inputText}
          placeholder={placeholder}
          className={cn(
            "h-8 w-full min-w-0 rounded-md border border-input bg-transparent py-1 pr-8 pl-8 text-sm outline-none transition-colors placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/50 disabled:cursor-not-allowed disabled:opacity-50 dark:bg-input/30",
            className,
          )}
        />
        {showClear ? (
          <button
            type="button"
            aria-label="Clear selection"
            onClick={(event) => {
              event.stopPropagation();
              setQuery(null);
              setActiveIndex(null);
              popover.setOpen(false);
              onClear();
            }}
            className="absolute top-1/2 right-2 flex size-4 -translate-y-1/2 items-center justify-center rounded-sm text-muted-foreground hover:text-foreground"
          >
            <XIcon className="size-3.5" />
          </button>
        ) : (
          <ChevronDownIcon className="pointer-events-none absolute top-1/2 right-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
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
              options={filteredOptions}
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
