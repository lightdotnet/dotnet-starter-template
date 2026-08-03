"use client";

import { useDeferredValue, useId, useMemo, useState, type ChangeEvent, type KeyboardEvent, type ReactNode } from "react";
import { Check, ChevronDownIcon, X } from "lucide-react";
import { FloatingPortal, useInteractions } from "@floating-ui/react";
import { cn } from "@/lib/shared/utils";
import { useFloatingPopover } from "@/components/foundation/use-floating-popover";
import { useListbox } from "@/components/foundation/use-listbox";
import { OptionsList } from "@/components/foundation/options-list";
import type { RenderOption, SelectOption, SelectOptionState } from "@/components/foundation/types";

export interface MultiSelectProps<TValue extends string = string> {
  options: SelectOption<TValue>[];
  value: TValue[];
  onValueChange: (value: TValue[]) => void;
  placeholder?: string;
  disabled?: boolean;
  id?: string;
  /** Renders one hidden input per selected value so this submits inside a native `<form>` (like a checkbox group). */
  name?: string;
  "aria-label"?: string;
  renderOption?: RenderOption<TValue>;
  renderChip?: (option: SelectOption<TValue>, onRemove: () => void) => ReactNode;
  /** Collapses extra chips into a "+N more" marker past this count. Unset shows all. */
  maxVisibleChips?: number;
  /** Defaults to a case-insensitive label substring match. */
  filter?: (option: SelectOption<TValue>, query: string) => boolean;
  className?: string;
}

function defaultFilter<TValue>(option: SelectOption<TValue>, query: string) {
  return option.label.toLowerCase().includes(query.trim().toLowerCase());
}

function defaultRenderOption<TValue>(option: SelectOption<TValue>, state: SelectOptionState) {
  return (
    <>
      <span
        aria-hidden="true"
        className={cn(
          "flex size-4 shrink-0 items-center justify-center rounded-sm border border-input",
          state.selected && "border-primary bg-primary text-primary-foreground",
        )}
      >
        {state.selected && <Check className="size-3" />}
      </span>
      <span className="truncate">{option.label}</span>
    </>
  );
}

/**
 * Multiple selection with chip display, filtered client-side. An async-
 * remote variant is not a separate component — compose this with
 * useAsyncOptions when a caller needs remote-searched multi-select rather
 * than shipping a seventh, near-duplicate component.
 */
export function MultiSelect<TValue extends string = string>({
  options,
  value,
  onValueChange,
  placeholder = "Select...",
  disabled,
  id,
  name,
  "aria-label": ariaLabel,
  renderOption = defaultRenderOption,
  renderChip,
  maxVisibleChips,
  filter = defaultFilter,
  className,
}: MultiSelectProps<TValue>) {
  const listboxId = useId();
  const [activeIndex, setActiveIndex] = useState<number | null>(null);
  const [query, setQuery] = useState("");
  const deferredQuery = useDeferredValue(query);

  const selectedOptions = useMemo(
    () => value.map((v) => options.find((option) => option.value === v)).filter((o): o is SelectOption<TValue> => !!o),
    [options, value],
  );

  const filteredOptions = useMemo(
    () => options.filter((option) => filter(option, deferredQuery)),
    [options, deferredQuery, filter],
  );

  // enableClick: false — see SearchSelect: the search input opens via
  // onFocus, and leaving click-to-toggle enabled would close it again on
  // the click that immediately follows that focus event.
  const popover = useFloatingPopover({
    placement: "bottom-start",
    role: "listbox",
    enableClick: false,
  });
  // enableTypeahead: false — see SearchSelect: this is a real text input
  // and useTypeahead would preventDefault every character keydown while open.
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

  function toggleOption(option: SelectOption<TValue>) {
    const isSelected = value.includes(option.value);
    onValueChange(isSelected ? value.filter((v) => v !== option.value) : [...value, option.value]);
  }

  function removeValue(targetValue: TValue) {
    onValueChange(value.filter((v) => v !== targetValue));
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") {
      event.preventDefault();
      if (activeIndex !== null) {
        const option = filteredOptions[activeIndex];
        if (option && !option.disabled) toggleOption(option);
      }
    } else if (event.key === "Escape") {
      popover.setOpen(false);
    } else if (event.key === "Backspace" && query === "" && selectedOptions.length > 0) {
      removeValue(selectedOptions[selectedOptions.length - 1].value);
    }
  }

  const visibleChips = maxVisibleChips ? selectedOptions.slice(0, maxVisibleChips) : selectedOptions;
  const overflowCount = selectedOptions.length - visibleChips.length;

  return (
    <>
      {name && value.map((v) => <input key={v} type="hidden" name={name} value={v} />)}
      <div
        ref={popover.refs.setReference}
        className={cn(
          "flex min-h-8 w-full flex-wrap items-center gap-1 rounded-md border border-input bg-transparent px-2 py-1 text-sm transition-colors focus-within:border-ring focus-within:ring-2 focus-within:ring-ring/50 dark:bg-input/30",
          disabled && "pointer-events-none cursor-not-allowed opacity-50",
          className,
        )}
      >
        {visibleChips.map((option) =>
          renderChip ? (
            <span key={option.value}>{renderChip(option, () => removeValue(option.value))}</span>
          ) : (
            <span
              key={option.value}
              className="inline-flex items-center gap-1 rounded-md bg-secondary px-1.5 py-0.5 text-xs text-secondary-foreground"
            >
              {option.label}
              <button
                type="button"
                onClick={() => removeValue(option.value)}
                aria-label={`Remove ${option.label}`}
                className="rounded-sm hover:bg-secondary-foreground/20"
              >
                <X className="size-3" />
              </button>
            </span>
          ),
        )}
        {overflowCount > 0 && (
          <span className="text-xs text-muted-foreground">+{overflowCount} more</span>
        )}
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
              setQuery("");
              popover.setOpen(false);
            },
            onKeyDown: handleKeyDown,
          })}
          id={id}
          disabled={disabled}
          role="combobox"
          aria-label={ariaLabel}
          aria-autocomplete="list"
          aria-expanded={popover.open}
          aria-controls={listboxId}
          aria-activedescendant={
            popover.open && activeIndex !== null ? `${listboxId}-option-${activeIndex}` : undefined
          }
          value={query}
          placeholder={selectedOptions.length === 0 ? placeholder : undefined}
          className="min-w-16 flex-1 border-0 bg-transparent p-0 text-sm outline-none placeholder:text-muted-foreground"
        />
        <span className="ml-auto flex shrink-0 items-center gap-1 self-center">
          {selectedOptions.length > 0 && (
            <button
              type="button"
              aria-label="Clear all"
              onClick={(event) => {
                event.stopPropagation();
                setQuery("");
                popover.setOpen(false);
                onValueChange([]);
              }}
              className="flex size-4 items-center justify-center rounded-sm text-muted-foreground hover:text-foreground"
            >
              <X className="size-3.5" />
            </button>
          )}
          <ChevronDownIcon className="pointer-events-none size-4 text-muted-foreground" />
        </span>
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
              isSelected={(option) => value.includes(option.value)}
              onSelect={toggleOption}
              renderOption={renderOption}
              multiselectable
            />
          </div>
        </FloatingPortal>
      )}
    </>
  );
}
