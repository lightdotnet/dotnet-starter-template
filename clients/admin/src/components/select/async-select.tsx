"use client";

import { useId, useState, type ChangeEvent, type KeyboardEvent } from "react";
import { ChevronDownIcon, SearchIcon, XIcon } from "lucide-react";
import { FloatingPortal, useInteractions } from "@floating-ui/react";
import { cn } from "@/lib/shared/utils";
import { useFloatingPopover } from "@/components/foundation/use-floating-popover";
import { useListbox } from "@/components/foundation/use-listbox";
import { useAsyncOptions } from "@/components/foundation/use-async-options";
import { OptionsList } from "@/components/foundation/options-list";
import type { RenderOption, SelectOption } from "@/components/foundation/types";

export interface AsyncSelectProps<TValue extends string = string> {
  /**
   * The full selected option, not just its value — an async select can't
   * derive a label for the current value from `fetcher` alone once the
   * matching search results have scrolled out of state, so the caller (who
   * already knows the label, e.g. from the record the value came from)
   * carries it here. This is the one place this library's "value in,
   * value+label out" convention deliberately differs from EntitySelect/
   * SearchSelect, mirroring the same tradeoff react-select's AsyncSelect makes.
   */
  value: SelectOption<TValue> | null;
  onValueChange: (option: SelectOption<TValue> | null) => void;
  fetcher: (query: string, signal: AbortSignal) => Promise<SelectOption<TValue>[]>;
  debounceMs?: number;
  /** Minimum characters before `fetcher` is called — avoids a backend round-trip per keystroke. Defaults to 3. */
  minChars?: number;
  placeholder?: string;
  disabled?: boolean;
  id?: string;
  /** Renders a hidden input so this select can submit inside a native `<form>`. */
  name?: string;
  "aria-label"?: string;
  renderOption?: RenderOption<TValue>;
  className?: string;
}

/** Remote-search select: debounced fetch + cancellation live in useAsyncOptions, not here. */
export function AsyncSelect<TValue extends string = string>({
  value,
  onValueChange,
  fetcher,
  debounceMs = 300,
  minChars = 3,
  placeholder = "Search...",
  disabled,
  id,
  name,
  "aria-label": ariaLabel,
  renderOption,
  className,
}: AsyncSelectProps<TValue>) {
  const listboxId = useId();
  const [activeIndex, setActiveIndex] = useState<number | null>(null);
  const [query, setQuery] = useState<string | null>(null);

  const inputText = query ?? value?.label ?? "";

  // enableClick: false — see SearchSelect: this trigger opens via onFocus,
  // and leaving click-to-toggle enabled would close it again on the click
  // that immediately follows that focus event.
  const popover = useFloatingPopover({
    placement: "bottom-start",
    role: "listbox",
    enableClick: false,
  });

  const { options, isLoading, error } = useAsyncOptions({
    fetcher,
    query: query ?? "",
    enabled: popover.open && query !== null,
    debounceMs,
    minChars,
  });

  // enableTypeahead: false — see SearchSelect: this is a real text input
  // and useTypeahead would preventDefault every character keydown while open.
  const { listRef, listNavigation, typeahead } = useListbox({
    context: popover.context,
    labels: options.map((option) => option.label),
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
    onValueChange(option);
    setQuery(null);
    setActiveIndex(null);
    popover.setOpen(false);
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") {
      event.preventDefault();
      if (activeIndex !== null) {
        const option = options[activeIndex];
        if (option && !option.disabled) handleSelect(option);
      }
    } else if (event.key === "Escape") {
      setQuery(null);
      setActiveIndex(null);
    }
  }

  const showClear = value !== null;

  return (
    <>
      {name && <input type="hidden" name={name} value={value?.value ?? ""} />}
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
              onValueChange(null);
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
            <div aria-live="polite" className="sr-only">
              {isLoading ? "Loading options" : error ? error : `${options.length} options available`}
            </div>
            <OptionsList
              id={listboxId}
              options={options}
              listRef={listRef}
              getItemProps={getItemProps}
              activeIndex={activeIndex}
              isSelected={(option) => option.value === value?.value}
              onSelect={handleSelect}
              renderOption={renderOption}
              isLoading={isLoading}
              error={error}
              emptyMessage={query && query.trim().length > 0 ? "No matches found." : "Type to search..."}
            />
          </div>
        </FloatingPortal>
      )}
    </>
  );
}
