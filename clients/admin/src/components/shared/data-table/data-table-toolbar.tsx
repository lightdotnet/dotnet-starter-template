"use client";

import { useEffect, useState } from "react";
import { Download, RefreshCw, Search, SlidersHorizontal } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/shared/utils";
import type { DataTableAction, DataTableColumn } from "@/components/shared/data-table/types";

const SEARCH_DEBOUNCE_MS = 400;

interface DataTableToolbarProps<TData> {
  actions?: DataTableAction[];
  columns: DataTableColumn<TData>[];
  hiddenColumnIds: Set<string>;
  onToggleColumn: (columnId: string) => void;
  searchValue?: string;
  onSearchChange?: (value: string) => void;
  searchPlaceholder?: string;
  onExport?: () => void;
  onRefresh?: () => void;
  isLoading?: boolean;
}

export function DataTableToolbar<TData>({
  actions,
  columns,
  hiddenColumnIds,
  onToggleColumn,
  searchValue,
  onSearchChange,
  searchPlaceholder = "Search...",
  onExport,
  onRefresh,
  isLoading,
}: DataTableToolbarProps<TData>) {
  const [searchText, setSearchText] = useState(() => searchValue ?? "");
  const [lastSearchValue, setLastSearchValue] = useState(searchValue ?? "");

  if ((searchValue ?? "") !== lastSearchValue) {
    setLastSearchValue(searchValue ?? "");
    setSearchText(searchValue ?? "");
  }

  useEffect(() => {
    if (!onSearchChange) return;
    if (searchText === (searchValue ?? "")) return;

    const timeout = setTimeout(() => onSearchChange(searchText), SEARCH_DEBOUNCE_MS);
    return () => clearTimeout(timeout);
    // eslint-disable-next-line react-hooks/exhaustive-deps -- only re-debounce on searchText changes
  }, [searchText]);

  const hideableColumns = columns.filter((column) => column.hideable !== false);

  return (
    <div className="flex flex-col gap-3">
      {actions && actions.length > 0 && (
        <div className="flex flex-wrap items-center gap-2">
          {actions.map((action) => {
            const Icon = action.icon;
            return (
              <Button key={action.key} variant={action.variant} onClick={action.onClick}>
                {Icon && <Icon data-icon="inline-start" />}
                {action.label}
              </Button>
            );
          })}
        </div>
      )}

      <div className="flex flex-wrap items-center justify-between gap-2">
        {onSearchChange ? (
          <label className="relative w-full max-w-sm">
            <span className="sr-only">Search</span>
            <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
            <input
              type="search"
              value={searchText}
              onChange={(event) => setSearchText(event.target.value)}
              placeholder={searchPlaceholder}
              className="h-9 w-full rounded-md border border-input bg-background py-1.5 pr-3 pl-9 text-sm outline-none transition-colors duration-fast ease-standard placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/50"
            />
          </label>
        ) : (
          <div />
        )}

        <div className="flex items-center gap-2">
          {onExport && (
            <Button variant="outline" size="sm" onClick={onExport}>
              <Download data-icon="inline-start" />
              Export
            </Button>
          )}
          {onRefresh && (
            <Button variant="outline" size="sm" onClick={onRefresh} disabled={isLoading}>
              <RefreshCw
                data-icon="inline-start"
                className={cn(isLoading && "animate-spin")}
              />
              Refresh
            </Button>
          )}
          {hideableColumns.length > 0 && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" size="sm">
                  <SlidersHorizontal data-icon="inline-start" />
                  Columns
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuLabel>Toggle columns</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {hideableColumns.map((column) => (
                  <DropdownMenuCheckboxItem
                    key={column.id}
                    checked={!hiddenColumnIds.has(column.id)}
                    onCheckedChange={() => onToggleColumn(column.id)}
                    onSelect={(event) => event.preventDefault()}
                    className={cn(hiddenColumnIds.has(column.id) && "opacity-50")}
                  >
                    {column.header}
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </div>
      </div>
    </div>
  );
}
