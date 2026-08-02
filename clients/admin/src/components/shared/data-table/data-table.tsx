"use client";

import { useState } from "react";
import { Inbox } from "lucide-react";
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
import type {
  DataTableAction,
  DataTableColumn,
  DataTableEmptyState,
  DataTableErrorState,
} from "@/components/shared/data-table/types";

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

  function toggleColumn(columnId: string) {
    setHiddenColumnIds((previous) => {
      const next = new Set(previous);
      if (next.has(columnId)) next.delete(columnId);
      else next.add(columnId);
      return next;
    });
  }

  const visibleColumns = columns.filter((column) => !hiddenColumnIds.has(column.id));
  const isEmpty = !isLoading && data.length === 0;

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

      {error ? (
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
          <TableHeader>
            <TableRow>
              {visibleColumns.map((column) => (
                <TableHead key={column.id} className={column.className}>
                  {column.header}
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
              : data.map((row) => (
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

      {!error && (
        <DataTablePagination
          pageNumber={pageNumber}
          totalPages={totalPages}
          totalRecords={totalRecords}
          onPageChange={onPageChange}
        />
      )}
    </div>
  );
}
