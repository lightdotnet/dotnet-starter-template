"use client";

import { useMemo, useState } from "react";
import { ArrowDown, ArrowUp, ArrowUpDown, Inbox } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { Empty, EmptyDescription, EmptyMedia, EmptyTitle } from "@/components/ui/empty";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { DataTableToolbar } from "@/components/shared/data-table/data-table-toolbar";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { cn } from "@/lib/shared/utils";
import type {
  DataTableAction,
  DataTableColumn,
  DataTableEmptyState,
  DataTableErrorState,
} from "@/components/shared/data-table/types";

interface SortState {
  columnId: string;
  direction: "asc" | "desc";
}

const SKELETON_ROWS = 5;

export interface DataTableProps<TData> {
  columns: DataTableColumn<TData>[];
  data: TData[];
  rowKey: (row: TData) => string;
  isLoading?: boolean;

  actions?: DataTableAction[];

  searchValue?: string;
  onSearchChange?: (value: string) => void;
  searchPlaceholder?: string;

  onExport?: () => void;
  onRefresh?: () => void;

  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
  onPageChange: (page: number) => void;

  emptyState?: DataTableEmptyState;
  error?: DataTableErrorState;
}

export function DataTable<TData>({
  columns,
  data,
  rowKey,
  isLoading,
  actions,
  searchValue,
  onSearchChange,
  searchPlaceholder,
  onExport,
  onRefresh,
  pageNumber,
  totalPages,
  totalRecords,
  onPageChange,
  emptyState,
  error,
}: DataTableProps<TData>) {
  const [hiddenColumnIds, setHiddenColumnIds] = useState<Set<string>>(new Set());
  const [sortState, setSortState] = useState<SortState | null>(null);

  function toggleColumn(columnId: string) {
    setHiddenColumnIds((previous) => {
      const next = new Set(previous);
      if (next.has(columnId)) next.delete(columnId);
      else next.add(columnId);
      return next;
    });
  }

  function toggleSort(columnId: string) {
    setSortState((previous) => {
      if (!previous || previous.columnId !== columnId) return { columnId, direction: "asc" };
      if (previous.direction === "asc") return { columnId, direction: "desc" };
      return null;
    });
  }

  const visibleColumns = columns.filter((column) => !hiddenColumnIds.has(column.id));
  const isEmpty = !isLoading && data.length === 0;
  // While a fresh request is in flight, prefer the table's own loading state
  // (skeleton rows) over a stale error left over from the previous render.
  const showError = !!error && !isLoading;

  const sortColumn = sortState
    ? columns.find((column) => column.id === sortState.columnId)
    : undefined;

  // Sorts only the `data` that was passed in — meaningless for callers that
  // paginate server-side and hand DataTable a single page at a time.
  const sortedData = useMemo(() => {
    if (!sortState || !sortColumn?.sortValue) return data;

    const { direction } = sortState;
    const { sortValue } = sortColumn;
    return [...data].sort((a, b) => {
      const aValue = sortValue(a);
      const bValue = sortValue(b);
      if (aValue < bValue) return direction === "asc" ? -1 : 1;
      if (aValue > bValue) return direction === "asc" ? 1 : -1;
      return 0;
    });
  }, [data, sortState, sortColumn]);

  return (
    <div className="flex flex-col gap-4">
      <DataTableToolbar
        actions={actions}
        columns={columns}
        hiddenColumnIds={hiddenColumnIds}
        onToggleColumn={toggleColumn}
        searchValue={searchValue}
        onSearchChange={onSearchChange}
        searchPlaceholder={searchPlaceholder}
        onExport={onExport}
        onRefresh={onRefresh}
        isLoading={isLoading}
      />

      {showError ? (
        <Alert variant="destructive">
          <AlertTitle>{error.title}</AlertTitle>
          {error.description && <AlertDescription>{error.description}</AlertDescription>}
        </Alert>
      ) : isEmpty ? (
        <Empty>
          <EmptyMedia variant="icon">
            {emptyState?.icon ? <emptyState.icon /> : <Inbox />}
          </EmptyMedia>
          <EmptyTitle>{emptyState?.title ?? "No records"}</EmptyTitle>
          {emptyState?.description && (
            <EmptyDescription>{emptyState.description}</EmptyDescription>
          )}
        </Empty>
      ) : (
        <Table>
          <TableHeader className="bg-muted">
            <TableRow>
              {visibleColumns.map((column) => (
                <TableHead
                  key={column.id}
                  className={cn("text-muted-foreground", column.className)}
                >
                  {column.sortable && column.sortValue ? (
                    <button
                      type="button"
                      onClick={() => toggleSort(column.id)}
                      className="inline-flex items-center gap-1 hover:text-foreground"
                    >
                      {column.header}
                      {sortState?.columnId === column.id ? (
                        sortState.direction === "asc" ? (
                          <ArrowUp className="size-3.5" />
                        ) : (
                          <ArrowDown className="size-3.5" />
                        )
                      ) : (
                        <ArrowUpDown className="size-3.5 opacity-50" />
                      )}
                    </button>
                  ) : (
                    column.header
                  )}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading
              ? Array.from({ length: SKELETON_ROWS }, (_, rowIndex) => (
                  <TableRow key={`skeleton-${rowIndex}`}>
                    {visibleColumns.map((column) => (
                      <TableCell key={column.id} className={column.className}>
                        <Skeleton className="h-4 w-full max-w-32" />
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              : sortedData.map((row) => (
                  <TableRow key={rowKey(row)}>
                    {visibleColumns.map((column) => (
                      <TableCell key={column.id} className={column.className}>
                        {column.cell(row)}
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
          </TableBody>
        </Table>
      )}

      {!showError && (
        <div className="border-t pt-4">
          <DataTablePagination
            pageNumber={pageNumber}
            totalPages={totalPages}
            totalRecords={totalRecords}
            onPageChange={onPageChange}
          />
        </div>
      )}
    </div>
  );
}
