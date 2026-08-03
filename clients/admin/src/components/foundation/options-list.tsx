"use client";

import { Loader2 } from "lucide-react";
import type { MutableRefObject } from "react";
import { cn } from "@/lib/shared/utils";
import { useVirtualList } from "@/components/foundation/use-virtual-list";
import type { RenderOption, SelectOption } from "@/components/foundation/types";

const DEFAULT_VIRTUALIZE_THRESHOLD = 50;
const DEFAULT_ROW_HEIGHT = 36;

export interface OptionsListProps<TValue> {
  /** Base id for this listbox; individual options are `${id}-option-${index}` for aria-activedescendant. */
  id: string;
  options: SelectOption<TValue>[];
  listRef: MutableRefObject<Array<HTMLElement | null>>;
  getItemProps: (userProps?: Record<string, unknown>) => Record<string, unknown>;
  activeIndex: number | null;
  isSelected: (option: SelectOption<TValue>) => boolean;
  onSelect: (option: SelectOption<TValue>, index: number) => void;
  renderOption?: RenderOption<TValue>;
  isLoading?: boolean;
  error?: string | null;
  emptyMessage?: string;
  /** Switches to virtualized rendering once the option count exceeds this. Defaults to 50. */
  virtualizeThreshold?: number;
  rowHeight?: number;
  className?: string;
  /** MultiSelect's listbox — aria-multiselectable belongs on the listbox, not the combobox input. */
  multiselectable?: boolean;
}

/**
 * Shared listbox rendering contract for every select-family component and
 * the Command Palette. Owns the virtualization threshold decision so no
 * individual component has to re-derive it, and sets aria-setsize/posinset
 * on every row so virtualization (which removes off-screen options from the
 * DOM) doesn't silently break a screen reader's item-count perception.
 */
export function OptionsList<TValue>({
  id,
  options,
  listRef,
  getItemProps,
  activeIndex,
  isSelected,
  onSelect,
  renderOption,
  isLoading,
  error,
  emptyMessage = "No options found.",
  virtualizeThreshold = DEFAULT_VIRTUALIZE_THRESHOLD,
  rowHeight = DEFAULT_ROW_HEIGHT,
  className,
  multiselectable,
}: OptionsListProps<TValue>) {
  const shouldVirtualize = options.length > virtualizeThreshold;
  const { scrollRef, virtualizer } = useVirtualList({
    count: shouldVirtualize ? options.length : 0,
    estimateSize: rowHeight,
  });

  function renderRow(option: SelectOption<TValue>, index: number) {
    const selected = isSelected(option);
    const active = activeIndex === index;
    return (
      <div
        key={String(option.value)}
        id={`${id}-option-${index}`}
        role="option"
        aria-selected={selected}
        aria-disabled={option.disabled || undefined}
        aria-setsize={options.length}
        aria-posinset={index + 1}
        data-active={active || undefined}
        ref={(node) => {
          listRef.current[index] = node;
        }}
        className={cn(
          "flex cursor-pointer items-center gap-1.5 rounded-md px-2 py-1.5 text-sm outline-hidden select-none",
          "data-[active]:bg-accent data-[active]:text-accent-foreground",
          option.disabled && "pointer-events-none opacity-50",
        )}
        {...getItemProps({
          // Prevent the browser from shifting focus to this (non-focusable)
          // row on mousedown — without this, clicking an option blurs the
          // still-focused combobox input a tick before onClick fires.
          onMouseDown: (event: { preventDefault: () => void }) => event.preventDefault(),
          onClick: () => {
            if (!option.disabled) onSelect(option, index);
          },
        })}
      >
        {renderOption ? (
          renderOption(option, { active, selected })
        ) : (
          <div className="flex min-w-0 flex-col">
            <span className="truncate">{option.label}</span>
            {option.description && (
              <span className="truncate text-xs text-muted-foreground">{option.description}</span>
            )}
          </div>
        )}
      </div>
    );
  }

  if (isLoading && options.length === 0) {
    return (
      <div
        className="flex items-center justify-center gap-2 px-2 py-4 text-sm text-muted-foreground"
        aria-live="polite"
      >
        <Loader2 className="size-4 animate-spin" />
        Loading...
      </div>
    );
  }

  if (error) {
    return (
      <div className="px-2 py-4 text-sm text-destructive" role="alert">
        {error}
      </div>
    );
  }

  if (options.length === 0) {
    return (
      <div className="px-2 py-4 text-sm text-muted-foreground" aria-live="polite">
        {emptyMessage}
      </div>
    );
  }

  if (!shouldVirtualize) {
    return (
      <div
        role="listbox"
        id={id}
        aria-multiselectable={multiselectable || undefined}
        className={cn("max-h-[inherit] overflow-y-auto p-1", className)}
      >
        {options.map((option, index) => renderRow(option, index))}
      </div>
    );
  }

  const virtualItems = virtualizer.getVirtualItems();

  return (
    <div
      ref={scrollRef}
      role="listbox"
      id={id}
      aria-multiselectable={multiselectable || undefined}
      className={cn("max-h-[inherit] overflow-y-auto p-1", className)}
    >
      <div style={{ height: virtualizer.getTotalSize(), position: "relative", width: "100%" }}>
        {virtualItems.map((virtualRow) => (
          <div
            key={virtualRow.key}
            data-index={virtualRow.index}
            ref={virtualizer.measureElement}
            style={{
              position: "absolute",
              top: 0,
              left: 0,
              width: "100%",
              transform: `translateY(${virtualRow.start}px)`,
            }}
          >
            {renderRow(options[virtualRow.index], virtualRow.index)}
          </div>
        ))}
      </div>
    </div>
  );
}
